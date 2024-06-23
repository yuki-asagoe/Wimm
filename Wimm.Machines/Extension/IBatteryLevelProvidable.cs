using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wimm.Machines.Extension
{
    /// <summary>
    /// バッテリ残量を出力できることを示すマシン拡張
    /// </summary>
    public interface IBatteryLevelProvidable : IMachineExtension
    {
        public Battery[] Batteries { get; }
        public record Battery(string Name,double Percentage,bool Charging);
    }
}
