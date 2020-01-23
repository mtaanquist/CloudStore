using CloudStoreApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace CloudStoreApp.Helpers
{
    public static class PreferencesHelper
    {
        public static void SavePreferences()
        {
            ExportPreferences("preferences.json");
        }

        public static void ExportPreferences(string path)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(path, JsonSerializer.Serialize(Preferences.Instance, options));
        }

        private static void ImportPreferences(string filePath)
        {
            if (string.IsNullOrEmpty(Preferences.Instance.CloudStoragePath))
                throw new ApplicationException(ResourceHelper.GetString("MissingCloudStoragePath"));

            var preferences = JsonSerializer.Deserialize<Preferences>(File.ReadAllText(filePath));

            // re-establish directories where possible
            foreach (var folder in preferences.StoredFolders)
            {
                var targetDirectoryPath = StorageHelper.GetTargetDirectory(folder);

                if (Directory.Exists(targetDirectoryPath) && Directory.Exists(folder.SourceDirectory))
                {
                    var sourceDirectoryInfo = new DirectoryInfo(folder.SourceDirectory);
                    if (!sourceDirectoryInfo.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    {
                        folder.LastException =
                            new IOException(ResourceHelper.GetString("BothPhysicalDirectoriesExistIOException"));
                        continue;
                    }

                    StorageHelper.DeleteFolder(folder.SourceDirectory);
                    NativeMethods.CreateSymbolicLink(folder.SourceDirectory, targetDirectoryPath, 0x1);
                }
                else if (Directory.Exists(targetDirectoryPath) && !Directory.Exists(folder.SourceDirectory))
                {
                    // target directory exists, but source directory doesn't; re-establish reparse point
                    NativeMethods.CreateSymbolicLink(folder.SourceDirectory, targetDirectoryPath, 0x1);
                }
                else if (!Directory.Exists(targetDirectoryPath) && Directory.Exists(folder.SourceDirectory))
                {
                    var sourceDirectoryInfo = new DirectoryInfo(folder.SourceDirectory);
                    if (sourceDirectoryInfo.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    {
                        folder.LastException =
                            new IOException(ResourceHelper.GetString("ReparsePointExistsWithoutTargetDirectory"));
                        continue;
                    }

                    StorageHelper.MoveFolder(folder);
                    NativeMethods.CreateSymbolicLink(folder.SourceDirectory, targetDirectoryPath, 0x1);
                }
                else
                {
                    // none of the directories exist, and we can't recover it from the list
                    folder.LastException = new DirectoryNotFoundException();
                }
            }

            // when done with the file, export it as a new file
            SavePreferences();
        }

        public static void LoadExistingPreferencesFile()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                FileName = @"preferences",
                DefaultExt = @".json",
                Filter = @"JSON files (.json)|*.json"
            };

            bool? result = openFileDialog.ShowDialog();
            if (result == true && File.Exists(openFileDialog.FileName))
            {
                ImportPreferences(openFileDialog.FileName);
            }
        }

        public static void ReadPreferencesFile()
        {
            if (File.Exists("preferences.json")) ImportPreferences("preferences.json");
        }

        public static void CreatePreferencesFile(string storageDirectoryPath)
        {
            if (!string.IsNullOrEmpty(storageDirectoryPath))
                Preferences.Instance.CloudStoragePath = storageDirectoryPath;

            Preferences.Instance.LastUpdated = DateTime.Now;
            SavePreferences();
        }
    }
}
