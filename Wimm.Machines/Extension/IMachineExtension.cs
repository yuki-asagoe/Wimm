using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wimm.Machines.Extension
{
    /// <summary>
    /// <para>
    /// マシンに <b>Wimm</b> から関与できるが <b>Machine</b>クラスに含められない拡張機能を提供する為のインターフェース。
    /// これ自体はメソッドを提供しない、
    /// <para>
    /// 使用時はこれの派生クラスを <b>Machine</b>実装クラスに実装させること
    /// </para>
    /// </para>
    /// 
    /// <example>
    /// 使用例
    /// <code>
    /// class ExampleMachine : Machine, IPowerVoltageProvidable
    /// 
    /// if(machine is IPowerVoltageProvidable extension){
    ///     output=extension.Voltage;
    /// }
    /// </code>
    /// </example>
    /// </summary>
    public interface IMachineExtension{}
}
