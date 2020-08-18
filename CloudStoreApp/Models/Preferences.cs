using System;
using System.Collections.Generic;

namespace CloudStoreApp.Models
{
    public sealed class Preferences
    {
        static Preferences()
        {
        }

        private Preferences()
        {
            if (StoredFolders == null)
                StoredFolders = new List<StoredFolder>();
        }

        public static Preferences Instance { get; set; } = new Preferences();

        public DateTime LastUpdated { get; set; }
        public string CloudStorePath { get; set; }
        public bool HideSourceDirectory { get; set; }

#pragma warning disable CA2227 // Collection properties should be read only
        public List<StoredFolder> StoredFolders { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }
}
