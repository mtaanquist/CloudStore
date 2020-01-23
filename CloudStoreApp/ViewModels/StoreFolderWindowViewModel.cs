using System.Windows;
using System.Windows.Input;
using CloudStoreApp.Commands;
using CloudStoreApp.Helpers;

namespace CloudStoreApp.ViewModels
{
    public class StoreFolderWindowViewModel : ViewModelBase
    {
        private string _sourceDirectoryPath;

        private string _targetFolderName;

        public StoreFolderWindowViewModel()
        {
            SelectFolderCommand = new RelayCommand(param =>
            {
                SourceDirectoryPath = StorageHelper.SelectFolder();
                TargetFolderName = StorageHelper.GetFolderName(SourceDirectoryPath);
            });

            SaveCommand = new RelayCommand(param =>
            {
                StorageHelper.StoreFolder(SourceDirectoryPath, TargetFolderName);
                if (param is Window window) window.Close();
            });
        }

        public string SourceDirectoryPath
        {
            get => _sourceDirectoryPath;
            set => SetProperty(ref _sourceDirectoryPath, value, nameof(SourceDirectoryPath));
        }

        public string TargetFolderName
        {
            get => _targetFolderName;
            set => SetProperty(ref _targetFolderName, value, nameof(TargetFolderName));
        }

        public ICommand SelectFolderCommand { get; }

        public ICommand SaveCommand { get; }

        public ICommand CancelCommand { get; } = new RelayCommand(param =>
        {
            if (param is Window window) window.Close();
        });
    }
}