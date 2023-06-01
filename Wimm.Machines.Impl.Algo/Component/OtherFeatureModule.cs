using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wimm.Machines.Impl.Algo.Component.RokkoOroshiMotorBoard;
using static Wimm.Machines.Impl.Algo.Algo;

namespace Wimm.Machines.Impl.Algo.Component
{
    public class OtherFeatureModule : Module
    {
        Algo Parent;
        double TopCameraAngle { get; set; }
        public OtherFeatureModule(string name, string description, Algo parent) : base(name, description)
        {
            Parent = parent;
            parent.SetTopCameraDefault += SetDefaultCameraCanData;
            parent.SetLiftDefault += SetDefaultLiftCanData;
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
        public void SetDefaultCameraCanData(TopCameraCanData data)
        {
            data.CanData[0] = (byte)TopCameraAngle;
        }
        public void SetDefaultLiftCanData(LiftControlCanData data)
        {
            data.CanData[(int)LiftDataIndex.Lift] = (byte)0;
        }

        public override string ModuleName => "その他モジュール";

        public override string ModuleDescription => "その他機能を実現するモジュール";
    }
    public class TopCameraCanData
    {
        CanID ID { get; } = new CanID()
        {
            DestinationAddress = (CanDestinationAddress)5,
            SourceAddress = CanDestinationAddress.BroadCast,
            MessageType = CanDataType.Command
        };
        public byte[] CanData { get; } = new byte[1];
        public void Send()
        {
            MotorBoardSupporter.Send(ID, CanData);
        }
    }
}
