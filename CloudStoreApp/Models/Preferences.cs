using System;
using System.Collections.Generic;

namespace CloudStoreApp.Models
{
    public sealed class Preferences
    {
        public Preferences()
        {
        }

        public DateTime LastUpdated { get; set; }
        public string CloudStorePath { get; set; } = string.Empty;
        public bool HideSourceDirectory { get; set; }

        public List<StoredFolder> StoredFolders { get; set; } = new();
    }
}
