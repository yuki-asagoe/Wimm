﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wimm.Common.Logging;

namespace Wimm.Machines
{
    public record MachineConstructorArgs(IntPtr HostingWindowHandle,ILogger Logger, DirectoryInfo MachineDirectory);
}
