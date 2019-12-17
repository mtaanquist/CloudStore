using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CloudStoreApp
{
    internal static class NativeMethods
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, int dwFlags);
    }
}
