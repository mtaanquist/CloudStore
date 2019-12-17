using System;
using System.Collections.Generic;
using System.Text;

namespace CloudStoreApp.ViewModels
{
    public class StoredFolderViewModel : ViewModelBase
    {
        private Guid _id;
        public Guid Id
        {
            get
            {
                return _id;
            }
            set
            {
                if (value == _id) return;
                _id = value;
                base.OnPropertyChanged(nameof(Id));
            }
        }

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value == _name) return;
                _name = value;
                base.OnPropertyChanged(nameof(Name));
            }
        }

        private string _sourceDirectory;
        public string SourceDirectory
        {
            get
            {
                return _sourceDirectory;
            }
            set
            {
                if (value == _sourceDirectory) return;
                _sourceDirectory = value;
                base.OnPropertyChanged(nameof(SourceDirectory));
            }
        }

        private string _targetDirectory;
        public string TargetDirectory
        {
            get
            {
                return _targetDirectory;
            }
            set
            {
                if (value == _targetDirectory) return;
                _targetDirectory = value;
                base.OnPropertyChanged(nameof(TargetDirectory));
            }
        }

        public bool IsMoved { get; set; }

        public Exception LastException { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
