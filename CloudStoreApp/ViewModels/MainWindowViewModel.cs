using System.Collections.ObjectModel;
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
                PreferencesHelper.CreatePreferencesFile();
            }
            else
            {
                Preferences? prefs = PreferencesHelper.LoadPreferences();
                FirstLaunch = !string.IsNullOrEmpty(prefs?.CloudStorePath);
            }

            StoreNewFolderCommand = new RelayCommand(param =>
            {
                StoreFolderWindow storeFolderWindow = new();
                _ = storeFolderWindow.ShowDialog();

                BuildStoredFoldersList();
            }, param => true);

            OpenPreferencesCommand = new RelayCommand(param =>
            {
                PreferencesWindow preferencesWindow = new();
                _ = preferencesWindow.ShowDialog();

                BuildStoredFoldersList();
            }, param => true);

            ImportPreferencesCommand = new RelayCommand(param =>
            {
                PreferencesHelper.LoadExistingPreferencesFile();
            }, param => true);

            BuildStoredFoldersList();
        }

        public bool FirstLaunch { get; set; }

        public ObservableCollection<StoredFolderViewModel> StoredFolders { get; } = new();

        #region Commands
        public ICommand StoreNewFolderCommand { get; }
        public ICommand OpenPreferencesCommand { get; }
        public ICommand ImportPreferencesCommand { get; }
        #endregion

        private void BuildStoredFoldersList()
        {
            Preferences prefs = PreferencesHelper.LoadPreferences();

            StoredFolders.Clear();

            if (prefs.StoredFolders == null || prefs.StoredFolders.Count == 0)
            {
                return;
            }

            prefs.StoredFolders.ForEach(storedFolder =>
            {
                StoredFolders.Add(new StoredFolderViewModel
                {
                    Id = storedFolder.Id,
                    Name = storedFolder.Name,
                    SourceDirectory = StorageHelper.GetSourceDirectory(storedFolder),
                    StoredFolder = storedFolder
                });
            });
        }
    }
}