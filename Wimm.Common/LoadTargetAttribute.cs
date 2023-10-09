using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wimm.Common
{
    /// <summary>
    /// Wimmによる読み込み対象になるMachineクラスに付与します。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class,AllowMultiple = false)]
    public class LoadTargetAttribute:Attribute{}
}
