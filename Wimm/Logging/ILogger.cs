using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Wimm.Logging
{
    public interface ILogger
    {
        public void Info(string message);
        public void Warn(string message);
        public void Error(string message);
        public static DirectoryInfo? GetLogDirectory()
        {
            var entryLocation = Assembly.GetEntryAssembly()?.Location;
            if (entryLocation is null) return null;
            var entryFile = new FileInfo(entryLocation);
            var entryDir = entryFile.Directory;
            return entryDir?.CreateSubdirectory("log");
        }
        public Common.Logging.ILogger ToCommonLogger(string name)
        {
            return new ForMachineLogger(this, name);
        }
        private class ForMachineLogger : Common.Logging.ILogger
        {
            public ForMachineLogger(ILogger logger,string prefix)
            {
                Logger = logger;
                Prefix = prefix;
            }
            ILogger Logger { get; }
            string Prefix { get; }
            public void Error(string message)
            {
                Logger.Error($"[{Prefix}]{message}");
            }

            public void Info(string message)
            {
                Logger.Info($"[{Prefix}]{message}");
            }

            public void Warn(string message)
            {
                Logger.Warn($"[{Prefix}]{message}");
            }
        }
    }
}
