using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Wimm.Machines.Component;
using Wimm.Machines.Impl.Algo.Component.RokkoOroshiMotorBoard;

namespace Wimm.Machines.Impl.Algo.Component
{
    public class AlgoArmServo : ServoMotor
    {
        Algo Parent { get; }
        ArmDataIndex DataIndex { get; }
        private double angle = 0;
        private double AngleMax { get; }
        private double AngleMin { get; }
        private static int MaxSpeed = 3;
        private double Angle
        {
            get { return angle; }
            set { angle = Math.Clamp(value,AngleMin,AngleMax); }
        }
        public AlgoArmServo(string name, string description,ArmDataIndex index,double angleMax,double angleMin, Algo algo) : base(name, description)
        {
            Parent = algo;
            AngleMax = Math.Max(angleMax,127);
            AngleMin = Math.Min(angleMin,-127);
            DataIndex = index;
        }

        public override Feature<Action<double, double>> SetAngleFeature 
            => new Feature<Action<double, double>>(
                SetAngleFeatureName,$"サーボの位置を指定します。アルゴは回転速度を無視します。角度は{AngleMin} ~ {AngleMax}",
                SetAngleImpl
            );
        public void SetAngleImpl(double angleDegree,double speed)
        {
            if(Parent.ControlProcess is AlgoControlProcess process)
            {
                Angle = angleDegree;
                process.ArmControlData.CanData[(int)DataIndex] = (byte)Angle;
            }
        }
        public override Feature<Action<double>> RotationFeature
            => new Feature<Action<double>>(
                RotationFeatureName,"サーボを回転させます。回転方向は実装依存です。速度は 1 ~ -1",
                Rotation
            );
        public void Rotation(double speed)
        {
            if(Parent.ControlProcess is AlgoControlProcess process)
            {
                speed = Math.Clamp(speed, -1, 1);
                Angle += MaxSpeed*speed;
                process.ArmControlData.CanData[(int)DataIndex] = (byte)Angle;
            }
        }
        public override string ModuleName => "アルゴ アーム モーター";

        public override string ModuleDescription => "アルゴの汎用アーム操作用サーボモーター";
    }
    
    public class ArmControlCanData
    {
        CanID ID { get; } = new CanID()
        {
            DestinationAddress = (CanDestinationAddress)6,
            SourceAddress = CanDestinationAddress.BroadCast,
            MessageType = CanDataType.Command
        };
        public byte[] CanData { get; } = new byte[5];
        public void Send()
        {
            MotorBoardSupporter.Send(ID, CanData);
        }
    }
    public enum ArmDataIndex
    {
        Roll=0,Pitch,Grip,Camera,Reboot
    }
}
