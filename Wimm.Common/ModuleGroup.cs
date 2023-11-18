using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wimm.Common
{
    /// <summary>
    /// グループ化されたモジュール群を表すレコード
    /// </summary>
    /// <param name="Name">グループの名前、全て小文字、もしくはスネークケースであることを推奨</param>
    /// <param name="Children">グループに含まれるグループ</param>
    /// <param name="Modules">グループに直接含まれるモジュール</param>
    public record ModuleGroup(
        string Name,
        ImmutableArray<ModuleGroup> Children,
        ImmutableArray<Module> Modules
    );
}
