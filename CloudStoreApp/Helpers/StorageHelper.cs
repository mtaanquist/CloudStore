using CloudStoreApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace CloudStoreApp.Helpers
{
    public static class StorageHelper
    {
        public static StoredFolder StoreFolder(string sourceDirectory, string targetFolderName)
        {
            var sourceDirectoryInfo = GetDirectoryInfo(sourceDirectory);
            var targetDirectory = Path.Combine(Preferences.Instance.CloudStoragePath, targetFolderName);
            var targetDirectoryInfo = GetDirectoryInfo(targetDirectory);

            if (Preferences.Instance.StoredFolders.Exists(f => f.SourceDirectory == sourceDirectory)) return null;
            
            var storedFolder = new StoredFolder
            {
                Id = Guid.NewGuid(),
                Name = sourceDirectoryInfo.Name,
                SourceDirectory = sourceDirectoryInfo.FullName,
                TargetDirectory = targetDirectoryInfo.FullName
            };

            MoveFolder(storedFolder);
            NativeMethods.CreateSymbolicLink(storedFolder.SourceDirectory, storedFolder.TargetDirectory, 0x1);

            storedFolder.IsMoved = true;
            Preferences.Instance.AddStoredFolder(storedFolder);

            if (Preferences.Instance.HideSourceDirectory) HideDirectory(storedFolder.SourceDirectory);
            PreferencesHelper.ExportPreferences();
            return storedFolder;
        }

        public static string SelectFolder()
        {
            using var folderDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                ShowNewFolderButton = false
            };

            var dialogResult = folderDialog.ShowDialog();
            if (dialogResult != System.Windows.Forms.DialogResult.OK) return string.Empty;
            return folderDialog.SelectedPath;
        }

        private static void MoveFolder(StoredFolder storedFolder)
        {
            try
            {
                Directory.Move(storedFolder.SourceDirectory, storedFolder.TargetDirectory);
            }
            catch (Exception ex)
            {
                storedFolder.LastException = ex;
            }
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

        private static DirectoryInfo GetDirectoryInfo(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            return new DirectoryInfo(path);
        }
    }
}
