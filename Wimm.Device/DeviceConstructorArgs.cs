using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wimm.Common.Logging;

namespace Wimm.Device
{
    public record DeviceConstructorArgs(IntPtr HostingWindowHandle, ILogger Logger, DirectoryInfo DeviceDirectory) { }
}
