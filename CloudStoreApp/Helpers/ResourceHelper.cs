using System;
using System.Reflection;
using System.Resources;
using System.Threading;

namespace CloudStoreApp.Helpers
{
    public static class ResourceHelper
    {
        public static string GetString(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            var rm = new ResourceManager("TextResources", Assembly.GetExecutingAssembly());
            var culture = Thread.CurrentThread.CurrentCulture;
            
            return rm.GetString(name, culture);
        }
    }
}
