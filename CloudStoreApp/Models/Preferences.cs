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
        }

        public static Preferences Instance { get; set; } = new Preferences();

        public DateTime LastUpdated { get; set; }
        public string CloudStoragePath { get; set; }
        public bool HideSourceDirectory { get; set; }
        public List<StoredFolder> StoredFolders { get; set; }
    }
}
