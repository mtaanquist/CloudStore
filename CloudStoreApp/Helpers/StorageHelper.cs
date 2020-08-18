using System;
using System.IO;
using System.Windows.Forms;
using CloudStoreApp.Models;

namespace CloudStoreApp.Helpers
{
    public static class StorageHelper
    {
        internal static void StoreFolder(string sourceDirectory, string targetFolderName)
        {
            if (Preferences.Instance.StoredFolders != null
                && Preferences.Instance.StoredFolders.Count != 0
                && Preferences.Instance.StoredFolders.Exists(f => f.SourceDirectory == sourceDirectory))
                return;

            var sourceDirectoryInfo = GetDirectoryInfo(sourceDirectory);
            string origSourceDirectoryName = sourceDirectoryInfo.FullName;
            var targetDirectoryInfo = GetDirectoryInfo(Preferences.Instance.CloudStorePath);

            string userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string sourceDirectoryName = sourceDirectoryInfo.FullName;
            if (sourceDirectoryName.Contains(userProfilePath, StringComparison.OrdinalIgnoreCase))
            {
                sourceDirectoryName = sourceDirectoryName.Replace(
                    userProfilePath, 
                    "%UserProfilePath%", 
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
                CopyFolder(storedFolder);
                DeleteFolder(origSourceDirectoryName);
            }
            else
            {
                MoveFolder(storedFolder);
            }
            NativeMethods.CreateSymbolicLink(origSourceDirectoryName, GetTargetDirectory(storedFolder), 0x1);

            storedFolder.IsMoved = true;
            Preferences.Instance.StoredFolders?.Add(storedFolder);

            if (Preferences.Instance.HideSourceDirectory)
                HideDirectory(origSourceDirectoryName);

            PreferencesHelper.SavePreferences();
        }

        internal static void RestoreFolder(StoredFolder folder)
        {
            var targetDirectoryPath = GetTargetDirectory(folder);
            var sourceDirectoryInfo = new DirectoryInfo(GetSourceDirectory(folder));

            if (Directory.Exists(targetDirectoryPath) && Directory.Exists(folder.SourceDirectory))
            {
                if (!sourceDirectoryInfo.Attributes.HasFlag(FileAttributes.ReparsePoint))
                {
                    folder.LastException =
                        new IOException(Resources.Strings.BothPhysicalDirectoriesExistIOException);
                }
                else
                {
                    DeleteFolder(folder.SourceDirectory);
                    NativeMethods.CreateSymbolicLink(folder.SourceDirectory, targetDirectoryPath, 0x1);
                }
            }
            else if (Directory.Exists(targetDirectoryPath) && !Directory.Exists(folder.SourceDirectory))
            {
                // target directory exists, but source directory doesn't; re-establish reparse point
                NativeMethods.CreateSymbolicLink(folder.SourceDirectory, targetDirectoryPath, 0x1);
            }
            else if (!Directory.Exists(targetDirectoryPath) && Directory.Exists(folder.SourceDirectory))
            {
                if (sourceDirectoryInfo.Attributes.HasFlag(FileAttributes.ReparsePoint))
                {
                    folder.LastException =
                        new IOException(Resources.Strings.ReparsePointExistsWithoutTargetDirectory);
                }
                else
                {
                    MoveFolder(folder);
                    NativeMethods.CreateSymbolicLink(folder.SourceDirectory, targetDirectoryPath, 0x1);
                }
            }
            else
            {
                // none of the directories exist, and we can't recover it from the list
                folder.LastException = new DirectoryNotFoundException();
            }

            Preferences.Instance.StoredFolders.Add(folder);
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

        private static void CopyFolder(StoredFolder folder)
        {
            var sourceDirectory = GetSourceDirectory(folder);
            var targetDirectory = GetTargetDirectory(folder);

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

        private static void MoveFolder(StoredFolder folder)
        {
            if (folder == null)
                throw new ArgumentNullException(nameof(folder));

            Directory.Move(folder.SourceDirectory, GetTargetDirectory(folder));
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

            if (folder.SourceDirectory.Contains("%UserProfilePath%", StringComparison.OrdinalIgnoreCase))
            {
                return folder.SourceDirectory.Replace(
                    "%UserProfilePath%",
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    StringComparison.OrdinalIgnoreCase
                );
            }

            return folder.SourceDirectory;
        }

        internal static string GetTargetDirectory(StoredFolder folder)
        {
            if (folder == null)
                throw new ArgumentNullException(nameof(folder));

            return Path.Combine(Preferences.Instance.CloudStorePath, folder.Name);
        }

        private static DirectoryInfo GetDirectoryInfo(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            return new DirectoryInfo(path);
        }
    }
}