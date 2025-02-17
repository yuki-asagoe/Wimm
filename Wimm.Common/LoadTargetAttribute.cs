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
    public class LoadTargetAttribute:Attribute{
        public Type? Target { get; }
        public LoadTargetAttribute() { }
        public LoadTargetAttribute(Type target) { Target = target; }
    }
}
