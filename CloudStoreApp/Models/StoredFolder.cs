using System;

namespace CloudStoreApp.Models
{
    public class StoredFolder
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string SourceDirectory { get; set; } = string.Empty;
        public bool IsMoved { get; set; }

        public bool HasError => LastException != null;
        public Exception? LastException { get; set; }
    }
}
