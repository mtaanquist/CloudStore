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

            var storedFolder = new StoredFolder
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
                MoveFolder(prefs, storedFolder);
            }
            NativeMethods.CreateSymbolicLink(
                origSourceDirectoryName, GetTargetDirectory(prefs, storedFolder), 0x1);

            storedFolder.IsMoved = true;
            prefs.StoredFolders?.Add(storedFolder);

            if (prefs.HideSourceDirectory)
            {
                HideDirectory(origSourceDirectoryName);
            }

            PreferencesHelper.SavePreferences(prefs);
        }

        internal static void RestoreFolder(Preferences prefs, StoredFolder folder)
        {
            var targetDirectoryPath = GetTargetDirectory(prefs, folder);
            var sourceDirectoryInfo = new DirectoryInfo(GetSourceDirectory(folder));

            if (Directory.Exists(targetDirectoryPath) && Directory.Exists(sourceDirectoryInfo.FullName))
            {
                if (!sourceDirectoryInfo.Attributes.HasFlag(FileAttributes.ReparsePoint))
                {
                    folder.LastException =
                        new IOException(Resources.Strings.BothPhysicalDirectoriesExistIOException);
                }
                else
                {
                    DeleteFolder(sourceDirectoryInfo.FullName);
                    NativeMethods.CreateSymbolicLink(sourceDirectoryInfo.FullName, targetDirectoryPath, 0x1);
                }
            }
            else if (Directory.Exists(targetDirectoryPath) && !Directory.Exists(sourceDirectoryInfo.FullName))
            {
                // target directory exists, but source directory doesn't; re-establish reparse point
                NativeMethods.CreateSymbolicLink(sourceDirectoryInfo.FullName, targetDirectoryPath, 0x1);
            }
            else if (!Directory.Exists(targetDirectoryPath) && Directory.Exists(sourceDirectoryInfo.FullName))
            {
                if (sourceDirectoryInfo.Attributes.HasFlag(FileAttributes.ReparsePoint))
                {
                    folder.LastException =
                        new IOException(Resources.Strings.ReparsePointExistsWithoutTargetDirectory);
                }
                else
                {
                    MoveFolder(prefs, folder);
                    NativeMethods.CreateSymbolicLink(sourceDirectoryInfo.FullName, targetDirectoryPath, 0x1);
                }
            }
            else
            {
                // none of the directories exist, and we can't recover it from the list
                folder.LastException = new DirectoryNotFoundException();
            }

            prefs.StoredFolders.Add(folder);
        }

        internal static string SelectFolder()
        {
            using var folderDialog = new FolderBrowserDialog
            {
                ShowNewFolderButton = false
            };

            var dialogResult = folderDialog.ShowDialog();
            return dialogResult != DialogResult.OK ? string.Empty : folderDialog.SelectedPath;
        }

        private static void CopyFolder(Preferences prefs, StoredFolder folder)
        {
            var sourceDirectory = GetSourceDirectory(folder);
            var targetDirectory = GetTargetDirectory(prefs, folder);

            CopyDirectory(sourceDirectory, targetDirectory);
        }

        private static void CopyDirectory(string sourceDirectory, string targetDirectory)
        {
            var directory = new DirectoryInfo(sourceDirectory);
            var directories = directory.GetDirectories();
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            // Get the files in the directory and copy them to the new location.
            var files = directory.GetFiles();
            foreach (var file in files)
            {
                var temporaryPath = Path.Combine(targetDirectory, file.Name);
                file.CopyTo(temporaryPath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            foreach (var subDirectory in directories)
            {
                var temporaryPath = Path.Combine(targetDirectory, subDirectory.Name);
                CopyDirectory(subDirectory.FullName, temporaryPath);
            }
        }

        private static void MoveFolder(Preferences prefs, StoredFolder folder)
        {
            if (folder == null)
                throw new ArgumentNullException(nameof(folder));

            Directory.Move(folder.SourceDirectory, GetTargetDirectory(prefs, folder));
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
            var directoryInfo = GetDirectoryInfo(path);
            return directoryInfo.Name;
        }

        internal static string GetSourceDirectory(StoredFolder folder)
        {
            if (folder == null)
                throw new ArgumentNullException(nameof(folder));

            if (folder.SourceDirectory.Contains(USER_PROFILE_PATH, StringComparison.OrdinalIgnoreCase))
            {
                return folder.SourceDirectory.Replace(
                    USER_PROFILE_PATH,
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    StringComparison.OrdinalIgnoreCase
                );
            }

            return folder.SourceDirectory;
        }

        internal static string GetTargetDirectory(Preferences prefs, StoredFolder folder)
        {
            if (folder == null)
                throw new ArgumentNullException(nameof(folder));

            return Path.Combine(prefs.CloudStorePath, folder.Name);
        }

        private static DirectoryInfo GetDirectoryInfo(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            return new DirectoryInfo(path);
        }
    }
}