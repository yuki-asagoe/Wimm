using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wimm.Machines.Logging;

namespace Wimm.Machines
{
    public record MachineConstructorArgs(IntPtr HostingWindowHandle,IMachineLogger Logger, DirectoryInfo MachineDirectory);
}
