using CloudStoreApp.Commands;
using CloudStoreApp.Helpers;
using CloudStoreApp.Models;
using System;
using System.Windows.Input;

namespace CloudStoreApp.ViewModels
{
    public class StoredFolderViewModel : ViewModelBase
    {
        private Guid _id;
        private string _name = string.Empty;
        private string _sourceDirectory = string.Empty;
        private string _targetDirectory = string.Empty;
        private StoredFolder _storedFolder = new();

        #region Commands
        public ICommand OpenFolderCommand { get; }
        public ICommand RemoveFolderCommand { get; }
        public ICommand RedoCommand { get; }
        #endregion

        public StoredFolderViewModel()
        {
            OpenFolderCommand = new RelayCommand(param =>
            {
                CommandHelper.ExecuteOpenFolder(this);
            }, param => true);

            RemoveFolderCommand = new RelayCommand(param =>
            {
                CommandHelper.ExecuteOpenFolder(this);
            }, param => true);

            RedoCommand = new RelayCommand(param =>
            {
                CommandHelper.ExecuteRedoFolder(this);
            }, param => true);
        }

        public Guid Id
        {
            get => _id;
            set
            {
                if (value == _id) return;
                _id = value;
                base.OnPropertyChanged(nameof(Id));
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (value == _name)
                {
                    return;
                }

                _name = value;
                base.OnPropertyChanged(nameof(Name));
            }
        }

        public string SourceDirectory
        {
            get => _sourceDirectory;
            set
            {
                if (value == _sourceDirectory)
                {
                    return;
                }

                _sourceDirectory = value;
                base.OnPropertyChanged(nameof(SourceDirectory));
            }
        }

        public string TargetDirectory
        {
            get => _targetDirectory;
            set
            {
                if (value == _targetDirectory)
                {
                    return;
                }

                _targetDirectory = value;
                base.OnPropertyChanged(nameof(TargetDirectory));
            }
        }

        public StoredFolder StoredFolder
        {
            get => _storedFolder;
            set
            {
                if (value == _storedFolder)
                {
                    return;
                }

                _storedFolder = value;
                base.OnPropertyChanged(nameof(TargetDirectory));
            }
        }

        public bool IsMoved { get; set; }

        public Exception? LastException { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}