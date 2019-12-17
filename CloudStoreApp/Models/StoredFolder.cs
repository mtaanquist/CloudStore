using System;

namespace CloudStoreApp.Models
{
    public class StoredFolder
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string SourceDirectory { get; set; }
        public string TargetDirectory { get; set; }
        public bool IsMoved { get; set; }

        public Exception LastException { get; set; }
    }
}
