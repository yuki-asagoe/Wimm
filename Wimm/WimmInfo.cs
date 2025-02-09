using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wimm
{
    internal static class WimmInfo
    {
        private static DirectoryInfo? AppFolder { get; set; }
        public static DirectoryInfo GetAppFolder()
        {
            return AppFolder ??= new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).CreateSubdirectory("Wimm");
        }
    }
}
