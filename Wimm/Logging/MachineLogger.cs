using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wimm.Common.Logging;

namespace Wimm.Logging
{
    internal class MachineLogger : ILogger
    {
        public MachineLogger(ILogger logger)
        {
            Logger = logger;
        }
        readonly ILogger Logger;
        public void Error(string message)
        {
            Logger.Error($"[Machine]{message}");
        }

        public void Info(string message)
        {
            Logger.Info($"[Machine]{message}");
        }

        public void Warn(string message)
        {
            Logger.Warn($"[Machine]{message}");
        }
    }
}
