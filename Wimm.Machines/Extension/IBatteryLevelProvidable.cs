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
        /// <summary>
        /// 現在のバッテリ残量を返す。
        /// </summary>
        /// <value>現在のバッテリ残量 単位は[%]</value>
        public double BatteryPercentage { get; }
    }
}
