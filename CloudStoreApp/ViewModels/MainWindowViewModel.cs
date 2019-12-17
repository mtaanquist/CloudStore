using CloudStoreApp.Commands;
using CloudStoreApp.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using CloudStoreApp.Models;
using System.Windows;

namespace CloudStoreApp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<StoredFolderViewModel> StoredFolders { get; } = new ObservableCollection<StoredFolderViewModel>();
        public bool IsLoaded { get; set; }

        public MainWindowViewModel()
        {
            StoreNewFolderCommand = new RelayCommand(
                param =>
                {
                    var storeFolderWindow = new StoreFolderWindow();
                    storeFolderWindow.ShowDialog();

                    BuildStoredFoldersList();
                },
                param => !string.IsNullOrEmpty(Preferences.Instance.CloudStoragePath)
            );

            IsLoaded = LoadPreferences();
            BuildStoredFoldersList();
        }

        public static bool LoadPreferences()
        {
            PreferencesHelper.ReadPreferencesFile();

            if (!string.IsNullOrEmpty(Preferences.Instance.CloudStoragePath)) return true;

            var dialogResult = MessageBoxHelper.ShowConfirmMessageBox(
                (string)Application.Current.FindResource("InitialSetupMessage"),
                (string)Application.Current.FindResource("InitialSetupCaption")
            );

            switch (dialogResult)
            {
                case MessageBoxResult.Yes:
                    PreferencesHelper.LoadExistingPreferencesFile();
                    break;
                case MessageBoxResult.No:
                    string cloudStoragePath = string.Empty;

                    dialogResult = MessageBoxHelper.ShowMessageBox(
                        (string)Application.Current.FindResource("SelectCloudStoragePathMessage"),
                        (string)Application.Current.FindResource("SelectCloudStoragePathCaption")
                    );

                    if (dialogResult == MessageBoxResult.OK) cloudStoragePath = StorageHelper.SelectFolder();

                    PreferencesHelper.CreatePreferencesFile(cloudStoragePath);
                    break;
                case MessageBoxResult.Cancel:
                    return false;
            }

            return true;
        }

        public void BuildStoredFoldersList()
        {
            StoredFolders.Clear();

            if (Preferences.Instance.StoredFolders == null || Preferences.Instance.StoredFolders.Count == 0) return;

            Preferences.Instance.StoredFolders.ForEach(storedFolder =>
            {
                StoredFolders.Add(new StoredFolderViewModel
                {
                    Id = storedFolder.Id,
                    Name = storedFolder.Name,
                    SourceDirectory = storedFolder.SourceDirectory,
                    TargetDirectory = storedFolder.TargetDirectory
                });
            });
        }

        public ICommand StoreNewFolderCommand { get; }
    }
}
