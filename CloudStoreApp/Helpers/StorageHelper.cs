using System;
using System.IO;
using System.Windows.Forms;
using CloudStoreApp.Models;

namespace CloudStoreApp.Helpers
{
    public static class StorageHelper
    {
        private const string USER_PROFILE_PATH = @"%UserProfilePath%";

        internal static void StoreFolder(string sourceDirectory, string targetFolderName)
        {
            Preferences prefs = PreferencesHelper.LoadPreferences();

            if (prefs.StoredFolders != null
                && prefs.StoredFolders.Count != 0
                && prefs.StoredFolders.Exists(f => f.SourceDirectory == sourceDirectory))
            {
                return;
            }

            DirectoryInfo sourceDirectoryInfo = GetDirectoryInfo(sourceDirectory);
            string origSourceDirectoryName = sourceDirectoryInfo.FullName;
            DirectoryInfo targetDirectoryInfo = GetDirectoryInfo(prefs.CloudStorePath);

            string userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string sourceDirectoryName = sourceDirectoryInfo.FullName;
            if (sourceDirectoryName.Contains(userProfilePath, StringComparison.OrdinalIgnoreCase))
            {
                sourceDirectoryName = sourceDirectoryName.Replace(
                    userProfilePath,
                    USER_PROFILE_PATH,
                    StringComparison.OrdinalIgnoreCase
                );
            }

            StoredFolder? storedFolder = new()
            {
                Id = Guid.NewGuid(),
                Name = targetFolderName,
                SourceDirectory = sourceDirectoryName
            };

            if (sourceDirectoryInfo.Root != targetDirectoryInfo.Root)
            {
                CopyFolder(prefs, storedFolder);
                DeleteFolder(origSourceDirectoryName);
            }
            else
            {
                MoveFolder(prefs.CloudStorePath, storedFolder);
            }

            _ = NativeMethods.CreateSymbolicLink(
                origSourceDirectoryName,
                GetTargetDirectory(prefs.CloudStorePath, storedFolder), 0x1);

            storedFolder.IsMoved = true;
            prefs.StoredFolders?.Add(storedFolder);

            if (prefs.HideSourceDirectory)
            {
                HideDirectory(origSourceDirectoryName);
            }

            PreferencesHelper.SavePreferences(prefs);
        }

        internal static bool RestoreFolder(string cloudStoreDirectoryPath, StoredFolder folder)
        {
            string? targetDirectoryPath = GetTargetDirectory(cloudStoreDirectoryPath, folder);
            DirectoryInfo? sourceDirectoryInfo = new(GetSourceDirectory(folder));

            bool targetDirectoryExists = Directory.Exists(targetDirectoryPath);
            bool sourceDirectoryExists = Directory.Exists(sourceDirectoryInfo.FullName);

            if (targetDirectoryExists && sourceDirectoryExists)
            {
                if (!sourceDirectoryInfo.Attributes.HasFlag(FileAttributes.ReparsePoint))
                {
                    folder.LastException =
                        new IOException(Resources.Strings.BothPhysicalDirectoriesExistIOException);
                }
                else
                {
                    DeleteFolder(sourceDirectoryInfo.FullName);
                }
            }
            else if (targetDirectoryExists && !sourceDirectoryExists)
            {
                DirectoryInfo? sourceParentDirectoryInfo = sourceDirectoryInfo.Parent;
                if (sourceParentDirectoryInfo is not null)
                {
                    string sourceParentDirectory = sourceParentDirectoryInfo.FullName;
                    if (!Directory.Exists(sourceParentDirectory))
                    {
                        _ = Directory.CreateDirectory(sourceParentDirectory);
                    }
                }
            }
            else if (!targetDirectoryExists && sourceDirectoryExists)
            {
                if (sourceDirectoryInfo.Attributes.HasFlag(FileAttributes.ReparsePoint))
                {
                    folder.LastException =
                        new IOException(Resources.Strings.ReparsePointExistsWithoutTargetDirectory);
                }
                else
                {
                    MoveFolder(cloudStoreDirectoryPath, folder);
                }
            }
            else
            {
                // none of the directories exist, and we can't recover it from the list
                folder.LastException = new DirectoryNotFoundException();
            }

            return NativeMethods.CreateSymbolicLink(
                sourceDirectoryInfo.FullName,
                targetDirectoryPath,
                0x1);
        }

        internal static void RemoveFolder(StoredFolder folder)
        {
            DeleteFolder(folder.SourceDirectory);
            Directory.Move(
                GetTargetDirectory(PreferencesHelper.GetCloudStoreDirectoryPath(), folder),
                GetSourceDirectory(folder));
        }

        internal static string SelectFolder()
        {
            using FolderBrowserDialog? folderDialog = new()
            {
                ShowNewFolderButton = false
            };

            DialogResult dialogResult = folderDialog.ShowDialog();
            return dialogResult != DialogResult.OK ? string.Empty : folderDialog.SelectedPath;
        }

        private static void CopyFolder(Preferences prefs, StoredFolder folder)
        {
            string? sourceDirectory = GetSourceDirectory(folder);
            string? targetDirectory = GetTargetDirectory(prefs.CloudStorePath, folder);

            CopyDirectory(sourceDirectory, targetDirectory);
        }

        private static void CopyDirectory(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo? directory = new(sourceDirectory);
            DirectoryInfo[]? directories = directory.GetDirectories();
            if (!Directory.Exists(targetDirectory))
            {
                _ = Directory.CreateDirectory(targetDirectory);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[]? files = directory.GetFiles();
            foreach (FileInfo? file in files)
            {
                string? temporaryPath = Path.Combine(targetDirectory, file.Name);
                _ = file.CopyTo(temporaryPath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            foreach (DirectoryInfo? subDirectory in directories)
            {
                string? temporaryPath = Path.Combine(targetDirectory, subDirectory.Name);
                CopyDirectory(subDirectory.FullName, temporaryPath);
            }
        }

        private static void MoveFolder(string cloudStoreDirectoryPath, StoredFolder folder)
        {
            if (folder == null)
                throw new ArgumentNullException(nameof(folder));

            Directory.Move(folder.SourceDirectory, GetTargetDirectory(cloudStoreDirectoryPath, folder));
        }

        private static void DeleteFolder(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            Directory.Delete(path, true);
        }

        private static void HideDirectory(string targetDirectory)
        {
            GetDirectoryInfo(targetDirectory).Attributes |= FileAttributes.Hidden;
        }

        internal static string GetFolderName(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            DirectoryInfo? directoryInfo = GetDirectoryInfo(path);
            return directoryInfo.Name;
        }

        internal static string GetSourceDirectory(StoredFolder folder)
        {
            if (folder == null)
                throw new ArgumentNullException(nameof(folder));

            return folder.SourceDirectory.Contains(USER_PROFILE_PATH, StringComparison.OrdinalIgnoreCase)
                ? folder.SourceDirectory.Replace(
                    USER_PROFILE_PATH,
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    StringComparison.OrdinalIgnoreCase
                )
                : folder.SourceDirectory;
        }

        internal static string GetTargetDirectory(string cloudStoreDirectoryPath, StoredFolder folder)
        {
            return folder == null
                ? throw new ArgumentNullException(nameof(folder))
                : Path.Combine(cloudStoreDirectoryPath, folder.Name);
        }

        private static DirectoryInfo GetDirectoryInfo(string path)
        {
            return string.IsNullOrEmpty(path) ? throw new ArgumentNullException(nameof(path)) : new DirectoryInfo(path);
        }
    }
}