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
        public VoltageInfo[] Voltages { get; }
        /// <param name="Voltage">現在の電圧値、単位は[V]です。</param>
        /// <param name="MinVoltage">
        /// 電圧の最小値<br/>目安量です、必ずしも <b>Voltage</b> の値がこれを超えないとは限りません
        /// </param>
        /// <param name="MaxVoltage">
        /// 電圧の最大値<br/>目安量です、必ずしも <b>Voltage</b> の値がこれを超えないとは限りません
        /// </param>
        public record VoltageInfo(string Name,double Voltage,double MinVoltage,double MaxVoltage);
    }
}
