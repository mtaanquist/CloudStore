using System.Collections.ObjectModel;
using System.Windows.Input;
using CloudStoreApp.Commands;
using CloudStoreApp.Helpers;
using CloudStoreApp.Models;

namespace CloudStoreApp.ViewModels;

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
            FirstLaunch = string.IsNullOrEmpty(prefs?.CloudStorePath);
        }

        StoreNewFolderCommand = new RelayCommand(_ =>
        {
            StoreFolderWindow storeFolderWindow = new();
            _ = storeFolderWindow.ShowDialog();
            BuildStoredFoldersList();
        });

        OpenPreferencesCommand = new RelayCommand(_ =>
        {
            PreferencesWindow preferencesWindow = new();
            _ = preferencesWindow.ShowDialog();
            BuildStoredFoldersList();
        });

        ImportPreferencesCommand = new RelayCommand(_ =>
        {
            PreferencesHelper.LoadExistingPreferencesFile();
            BuildStoredFoldersList();
        });

        BuildStoredFoldersList();
    }

    public bool FirstLaunch { get; set; }

    public ObservableCollection<StoredFolderViewModel> StoredFolders { get; } = [];

    #region Commands
    public ICommand StoreNewFolderCommand { get; }
    public ICommand OpenPreferencesCommand { get; }
    public ICommand ImportPreferencesCommand { get; }
    #endregion

    private void BuildStoredFoldersList()
    {
        Preferences prefs = PreferencesHelper.LoadPreferences();

        StoredFolders.Clear();

        if (prefs.StoredFolders.Count == 0)
            return;

        foreach (StoredFolder storedFolder in prefs.StoredFolders)
        {
            StoredFolders.Add(new StoredFolderViewModel
            {
                Id = storedFolder.Id,
                Name = storedFolder.Name,
                SourceDirectory = StorageHelper.GetSourceDirectory(storedFolder),
                StoredFolder = storedFolder
            });
        }
    }
}