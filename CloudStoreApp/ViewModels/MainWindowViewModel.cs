using System.Collections.ObjectModel;
using System.Windows.Input;
using CloudStoreApp.Commands;
using CloudStoreApp.Models;

namespace CloudStoreApp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
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

            BuildStoredFoldersList();
        }

        public ObservableCollection<StoredFolderViewModel> StoredFolders { get; } =
            new ObservableCollection<StoredFolderViewModel>();

        public ICommand StoreNewFolderCommand { get; }

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
                    SourceDirectory = storedFolder.SourceDirectory
                });
            });
        }
    }
}