using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using CloudStoreApp.Commands;
using CloudStoreApp.Helpers;
using CloudStoreApp.Models;

namespace CloudStoreApp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            // determine if this is first launch
            FirstLaunch = !PreferencesHelper.PreferencesFileExists();
            if (FirstLaunch)
            {
                // on first launch, create a new preferences file
                PreferencesHelper.CreatePreferencesFile("preferences.json");
            }
            else
            {
                PreferencesHelper.LoadPreferences();
                FirstLaunch = !string.IsNullOrEmpty(Preferences.Instance.CloudStorePath);
            }

            StoreNewFolderCommand = new RelayCommand(
                param =>
                {
                    var storeFolderWindow = new StoreFolderWindow();
                    storeFolderWindow.ShowDialog();

                    BuildStoredFoldersList();
                },
                param => true
            );

            OpenPreferencesCommand = new RelayCommand(
                param =>
                {
                    var preferencesWindow = new PreferencesWindow();
                    preferencesWindow.ShowDialog();

                    BuildStoredFoldersList();
                }
            );

            BuildStoredFoldersList();
        }

        public bool FirstLaunch { get; set; }

        public ObservableCollection<StoredFolderViewModel> StoredFolders { get; } =
            new ObservableCollection<StoredFolderViewModel>();

        #region Commands
        public ICommand StoreNewFolderCommand { get; }
        public ICommand OpenPreferencesCommand { get; }
        #endregion

        private void BuildStoredFoldersList()
        {
            StoredFolders.Clear();

            if (Preferences.Instance.StoredFolders == null || Preferences.Instance.StoredFolders.Count == 0) return;

            Preferences.Instance.StoredFolders.ForEach(storedFolder =>
            {
                StoredFolders.Add(new StoredFolderViewModel
                {
                    Id = storedFolder.Id,
                    Name = storedFolder.Name,
                    SourceDirectory = StorageHelper.GetSourceDirectory(storedFolder)
                });
            });
        }
    }
}