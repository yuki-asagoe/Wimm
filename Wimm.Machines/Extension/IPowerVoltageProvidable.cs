using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wimm.Machines.Extension
{
    /// <summary>
    /// 電源電圧を出力できることを示すマシン拡張
    /// 電圧単位は V
    /// </summary>
    public interface IPowerVoltageProvidable : IMachineExtension
    {
        /// <summary>
        /// 電圧の最大値<br/>
        /// 目安量です、必ずしも <b>Voltage</b> の値がこれを超えないとは限りません
        /// </summary>
        public double MaxVoltage { get; }
        /// <summary>
        /// 電圧の最小値<br/>
        /// 目安量です、必ずしも <b>Voltage</b> の値がこれを超えないとは限りません
        /// </summary>
        public double MinVoltage { get; }
        public double Voltage { get; }
    }
}
