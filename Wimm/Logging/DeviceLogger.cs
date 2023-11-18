using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wimm.Common.Logging;

namespace Wimm.Logging
{
    internal class DeviceLogger : ILogger
    {
        public DeviceLogger(ILogger logger,string name)
        {
            Logger = logger;
            Name = name;
        }
        readonly ILogger Logger;
        readonly string Name;
        public void Error(string message)
        {
            Logger.Error($"[Device:{Name}]{message}");
        }

        public void Info(string message)
        {
            Logger.Info($"[Device:{Name}]{message}");
        }

        public void Warn(string message)
        {
            Logger.Warn($"[Device:{Name}]{message}");
        }
    }
}
