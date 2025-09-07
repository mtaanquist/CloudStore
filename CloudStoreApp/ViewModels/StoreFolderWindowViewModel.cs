using System.IO;
using System.Windows;
using System.Windows.Input;
using CloudStoreApp.Commands;
using CloudStoreApp.Helpers;
using CloudStoreApp.Models;

namespace CloudStoreApp.ViewModels;

public class StoreFolderWindowViewModel : ViewModelBase
{
    private string _sourceDirectoryPath = string.Empty;
    private string _targetFolderName = string.Empty;

    public StoreFolderWindowViewModel()
    {
        Preferences prefs = PreferencesHelper.LoadPreferences();
        if (string.IsNullOrEmpty(prefs.CloudStorePath))
        {
            throw new DirectoryNotFoundException();
        }

        SelectFolderCommand = new RelayCommand(_ =>
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