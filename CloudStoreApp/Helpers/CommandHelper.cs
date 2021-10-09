using CloudStoreApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoreApp.Helpers
{
    public class CommandHelper
    {
        public static void ExecuteOpenFolder(StoredFolderViewModel storedFolder)
        {
            _ = Process.Start("explorer.exe", storedFolder.TargetDirectory);
        }

        public static void ExecuteRemoveFolder(StoredFolderViewModel storedFolder)
        {
            StorageHelper.RemoveFolder(storedFolder.StoredFolder);
        }

        public static void ExecuteRedoFolder(StoredFolderViewModel storedFolder)
        {
            StorageHelper.RestoreFolder(
                PreferencesHelper.GetCloudStoreDirectoryPath(), 
                storedFolder.StoredFolder);
        }
    }
}
