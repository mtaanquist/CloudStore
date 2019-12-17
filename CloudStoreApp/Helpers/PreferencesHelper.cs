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
        public static void ExportPreferences()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText("preferences.json", JsonSerializer.Serialize(Preferences.Instance, options));
        }

        private static void ImportPreferences(string filePath)
        {
            Preferences.Instance = JsonSerializer.Deserialize<Preferences>(File.ReadAllText(filePath));
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
            if (!string.IsNullOrEmpty(storageDirectoryPath)) Preferences.Instance.CloudStoragePath = storageDirectoryPath;
            Preferences.Instance.LastUpdated = DateTime.Now;
            ExportPreferences();
        }
    }
}
