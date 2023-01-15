using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wimm.Machines.Impl.Algo.Component
{
    public class OtherFeatureModule : Module
    {
        Algo Parent;
        public OtherFeatureModule(string name, string description, Algo parent) : base(name, description)
        {
            Parent = parent;
            Features = ImmutableArray.Create(
                new Feature<Delegate>(
                    "reset_arm", "アーム角度のリセットを要求します。", ResetArm
                )
            );
        }
        public void ResetArm()
        {
            if (Parent.ControlProcess is AlgoControlProcess process)
            {
                process.ArmControlData.CanData[(int)ArmDataIndex.Reboot] = 1;
            }
        }

        public override string ModuleName => "その他モジュール";

        public override string ModuleDescription => "その他機能を実現するモジュール";
    }
}
