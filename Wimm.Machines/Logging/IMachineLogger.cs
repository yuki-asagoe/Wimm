using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wimm.Machines.Logging
{
    public interface IMachineLogger
    {
        public void Info(string message);
        public void Warn(string message);
        public void Error(string message);
    }
}
