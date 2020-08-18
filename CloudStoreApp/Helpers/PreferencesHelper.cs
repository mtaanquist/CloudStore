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
        public static bool SelectCloudStorePath()
        {
            if (!File.Exists("preferences.json"))
                throw new FileNotFoundException();

            Preferences.Instance.CloudStorePath = StorageHelper.SelectFolder();
            if (string.IsNullOrEmpty(Preferences.Instance.CloudStorePath))
                return false;

            SavePreferences();
            return true;
        }

        public static void SavePreferences()
        {
            Preferences.Instance.LastUpdated = DateTime.Now;
            ExportPreferences("preferences.json");
        }

        public static void ExportPreferences(string path)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(path, JsonSerializer.Serialize(Preferences.Instance, options));
        }

        private static void ImportPreferences(string filePath)
        {
            if (string.IsNullOrEmpty(Preferences.Instance.CloudStorePath))
                throw new ApplicationException(Resources.Strings.MissingCloudStoragePath); 

            var preferences = JsonSerializer.Deserialize<Preferences>(File.ReadAllText(filePath));
            Preferences.Instance.StoredFolders.Clear();

            // re-establish directories where possible
            foreach (var folder in preferences.StoredFolders)
            {
                StorageHelper.RestoreFolder(folder);
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

        public static void LoadPreferences()
        {
            if (PreferencesFileExists())
            {
                Preferences.Instance = JsonSerializer.Deserialize<Preferences>(File.ReadAllText("preferences.json"));
            }
        }

        public static bool PreferencesFileExists()
        {
            return File.Exists("preferences.json");
        }

        public static void CreatePreferencesFile(string storageDirectoryPath)
        {
            if (!string.IsNullOrEmpty(storageDirectoryPath))
            {
                Preferences.Instance.CloudStorePath = storageDirectoryPath;
            }

            SavePreferences();
        }
    }
}
