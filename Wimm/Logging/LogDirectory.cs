﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Wimm.Logging
{
    public static class LogDirectory
    {
        public static DirectoryInfo? GetLogDirectory()
        {
            var entryLocation = Assembly.GetEntryAssembly()?.Location;
            if (entryLocation is null) return null;
            var entryFile = new FileInfo(entryLocation);
            var entryDir = entryFile.Directory;
            return entryDir?.CreateSubdirectory("log");
        }
    }
}
