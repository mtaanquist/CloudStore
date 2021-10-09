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
        private const string PREFERENCES_FILENAME = "preferences.json";

        public static bool SelectCloudStorePath()
        {
            Preferences prefs = LoadPreferences();

            string cloudStorePath = StorageHelper.SelectFolder();
            if (string.IsNullOrEmpty(cloudStorePath))
            {
                return false;
            }
            prefs.CloudStorePath = cloudStorePath;

            SavePreferences(prefs);
            return true;
        }

        public static void SavePreferences(Preferences prefs)
        {
            prefs.LastUpdated = DateTime.Now;
            ExportPreferences(prefs, PREFERENCES_FILENAME);
        }

        public static void ExportPreferences(Preferences prefs, string path)
        {
            JsonSerializerOptions? options = new() { WriteIndented = true };
            File.WriteAllText(path, JsonSerializer.Serialize(prefs, options));
        }

        private static void ImportPreferences(string filePath)
        {
            Preferences prefs = LoadPreferences();

            if (string.IsNullOrEmpty(prefs.CloudStorePath))
            {
                throw new ApplicationException(Resources.Strings.MissingCloudStoragePath);
            }

            string content = File.ReadAllText(filePath);
            Preferences? newPrefs = JsonSerializer.Deserialize<Preferences>(content);
            if (newPrefs is null)
            {
                throw new FileLoadException(filePath);
            }

            prefs.StoredFolders.Clear();

            // re-establish directories where possible
            foreach (StoredFolder? folder in newPrefs.StoredFolders)
            {
                StorageHelper.RestoreFolder(prefs.CloudStorePath, folder);
                prefs.StoredFolders.Add(folder);
            }

            // when done with the file, export it as a new file
            SavePreferences(newPrefs);
        }

        public static void LoadExistingPreferencesFile()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new()
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

        public static Preferences LoadPreferences()
        {
            if (!PreferencesFileExists())
            {
                throw new FileNotFoundException(PREFERENCES_FILENAME);
            }

            string? content = File.ReadAllText(PREFERENCES_FILENAME);
            Preferences? preferences = JsonSerializer.Deserialize<Preferences>(content);
            return preferences is null ? throw new FileLoadException() : preferences;
        }

        public static bool PreferencesFileExists()
        {
            return File.Exists(PREFERENCES_FILENAME);
        }

        public static void CreatePreferencesFile()
        {
            Preferences prefs = new();
            JsonSerializerOptions? options = new() { WriteIndented = true };
            File.WriteAllText(PREFERENCES_FILENAME, JsonSerializer.Serialize(prefs, options));
        }

        public static string GetCloudStoreDirectoryPath()
        {
            Preferences prefs = LoadPreferences();
            return prefs is null ? string.Empty : prefs.CloudStorePath;
        }
    }
}
