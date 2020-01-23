using System;
using System.IO;
using System.Windows.Forms;
using CloudStoreApp.Models;

namespace CloudStoreApp.Helpers
{
    public static class StorageHelper
    {
        public static void StoreFolder(string sourceDirectory, string targetFolderName)
        {
            var sourceDirectoryInfo = GetDirectoryInfo(sourceDirectory);

            if (Preferences.Instance.StoredFolders != null && Preferences.Instance.StoredFolders.Count != 0 &&
                Preferences.Instance.StoredFolders.Exists(f => f.SourceDirectory == sourceDirectory))
                return;

            var storedFolder = new StoredFolder
            {
                Id = Guid.NewGuid(),
                Name = targetFolderName,
                SourceDirectory = sourceDirectoryInfo.FullName
            };

            MoveFolder(storedFolder);
            NativeMethods.CreateSymbolicLink(storedFolder.SourceDirectory, GetTargetDirectory(storedFolder), 0x1);

            storedFolder.IsMoved = true;
            Preferences.Instance.StoredFolders?.Add(storedFolder);

            if (Preferences.Instance.HideSourceDirectory) HideDirectory(storedFolder.SourceDirectory);
            PreferencesHelper.SavePreferences();
        }

        public static string SelectFolder()
        {
            using var folderDialog = new FolderBrowserDialog
            {
                ShowNewFolderButton = false
            };

            var dialogResult = folderDialog.ShowDialog();
            return dialogResult != DialogResult.OK ? string.Empty : folderDialog.SelectedPath;
        }

        public static void MoveFolder(StoredFolder folder)
        {
            if (folder == null)
                throw new ArgumentNullException(nameof(folder));

            Directory.Move(folder.SourceDirectory, GetTargetDirectory(folder));
        }

        public static void DeleteFolder(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            Directory.Delete(path);
        }

        private static void HideDirectory(string targetDirectory)
        {
            GetDirectoryInfo(targetDirectory).Attributes |= FileAttributes.Hidden;
        }

        public static string GetFolderName(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            var directoryInfo = GetDirectoryInfo(path);
            return directoryInfo.Name;
        }

        public static string GetTargetDirectory(StoredFolder folder)
        {
            if (folder == null)
                throw new ArgumentNullException(nameof(folder));

            return Path.Combine(Preferences.Instance.CloudStoragePath, folder.Name);
        }

        private static DirectoryInfo GetDirectoryInfo(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            return new DirectoryInfo(path);
        }
    }
}