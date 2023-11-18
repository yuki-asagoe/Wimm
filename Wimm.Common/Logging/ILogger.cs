using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wimm.Common.Logging
{
    public interface ILogger
    {
        public void Info(string message);
        public void Warn(string message);
        public void Error(string message);
    }
}
