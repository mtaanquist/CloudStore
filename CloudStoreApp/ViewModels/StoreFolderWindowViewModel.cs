using CloudStoreApp.Commands;
using CloudStoreApp.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace CloudStoreApp.ViewModels
{
    public class StoreFolderWindowViewModel : ViewModelBase
    {
        private string _sourceDirectoryPath;
        public string SourceDirectoryPath
        {
            get => _sourceDirectoryPath;
            set => SetProperty(ref _sourceDirectoryPath, value, nameof(SourceDirectoryPath));
        }

        private string _targetFolderName;
        public string TargetFolderName
        {
            get => _targetFolderName;
            set => SetProperty(ref _targetFolderName, value, nameof(TargetFolderName));
        }

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
                if (param is System.Windows.Window) (param as System.Windows.Window).Close();
            });
        }

        public ICommand SelectFolderCommand { get; }

        public ICommand SaveCommand { get; } 

        public ICommand CancelCommand { get; } = new RelayCommand(param =>
        {
            if (param is System.Windows.Window) (param as System.Windows.Window).Close();
        });
    }
}
