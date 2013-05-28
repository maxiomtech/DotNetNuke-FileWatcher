using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InspectorIT.FileWatcher.Components.Common
{
    public class Utils
    {
        public static string GetRelativePath(string portalRoot, string filePath)
        {
            return filePath.Replace(portalRoot, "").Replace("\\","/");
        }

    }
}