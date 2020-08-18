using CloudStoreApp.Commands;
using CloudStoreApp.Helpers;
using CloudStoreApp.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace CloudStoreApp.ViewModels
{
    public class PreferencesWindowViewModel : ViewModelBase
    {
        private string _cloudStorePath;

        public PreferencesWindowViewModel()
        {
            // ensure we have a preferences file
            if (!PreferencesHelper.PreferencesFileExists())
            {
                PreferencesHelper.CreatePreferencesFile("preferences.json");
            }

            PreferencesHelper.LoadPreferences();
            CloudStorePath = Preferences.Instance.CloudStorePath;

            SelectFolderCommand = new RelayCommand(param =>
            {
                CloudStorePath = StorageHelper.SelectFolder();
            });

            ImportPreferencesCommand = new RelayCommand(param =>
            {
                PreferencesHelper.LoadExistingPreferencesFile();
            });

            SaveCommand = new RelayCommand(param =>
            {
                SaveChanges();
                if (param is Window window) window.Close();
            });
        }

        #region Properties
        public string CloudStorePath
        {
            get => _cloudStorePath;
            set => SetProperty(ref _cloudStorePath, value, nameof(CloudStorePath));
        }
        #endregion

        #region Methods
        private void SaveChanges()
        {
            Preferences.Instance.CloudStorePath = CloudStorePath;
            PreferencesHelper.SavePreferences();
        }
        #endregion

        #region Commands
        public ICommand SelectFolderCommand { get; }

        public ICommand ImportPreferencesCommand { get; }

        public ICommand SaveCommand { get; }

        public ICommand CancelCommand { get; } = new RelayCommand(param =>
        {
            if (param is Window window) window.Close();
        });
        #endregion
    }
}
