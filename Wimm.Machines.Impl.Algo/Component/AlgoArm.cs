using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using Wimm.Machines.Component;
using Wimm.Machines.Impl.Algo.Component.RokkoOroshiMotorBoard;
using static Wimm.Machines.Impl.Algo.Algo;

namespace Wimm.Machines.Impl.Algo.Component
{
    public class AlgoArmServo : ServoMotor
    {
        Algo Parent { get; }
        ArmDataIndex DataIndex { get; }
        private static int MaxSpeed = 10;
        public AlgoArmServo(string name, string description,ArmDataIndex index,double angleMax,double angleMin, Algo algo) : base(name, description, Math.Max(angleMin, -127), Math.Min(angleMax, 127))
        {
            Parent = algo;
            DataIndex = index;
            algo.SetArmDefault += SetDefaultCanData;
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
                Angle = Math.Clamp(angleDegree,AngleMin,AngleMax);
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
                Angle += MaxSpeed*speed*Parent.SpeedModifier;
                process.ArmControlData.CanData[(int)DataIndex] = (byte)Angle;
            }
        }
        public void SetDefaultCanData(ArmControlCanData data)
        {
            data.CanData[(int)DataIndex] = (byte)Angle;
        }
        public override string ModuleName => "アルゴ アーム モーター";

        public override string ModuleDescription => "アルゴの汎用アーム操作用サーボモーター";
    }
    public class AlgoArmRootServo : ServoMotor
    {
        Algo Parent { get; }
        private static double MaxSpeed = 0.5;
        public override Feature<Action<double, double>> SetAngleFeature
            => new Feature<Action<double, double>>(
                SetAngleFeatureName, $"サーボの位置を指定します。アルゴは回転速度を無視します。角度は{AngleMin} ~ {AngleMax}",
                SetAngleImpl
            );
        public void SetAngleImpl(double angleDegree, double speed)
        {
            if (Parent.ControlProcess is AlgoControlProcess process)
            {
                Angle = Math.Clamp(angleDegree, AngleMin, AngleMax);
                process.LiftControlCanData.CanData[1] = (byte)Angle;
            }
        }
        public override Feature<Action<double>> RotationFeature
            => new Feature<Action<double>>(
                RotationFeatureName, "サーボを回転させます。回転方向は実装依存です。速度は 1 ~ -1",
                Rotation
            );
        public void Rotation(double speed)
        {
            if (Parent.ControlProcess is AlgoControlProcess process)
            {
                speed = Math.Clamp(speed, -1, 1);
                Angle += MaxSpeed * speed * Parent.SpeedModifier;
                process.LiftControlCanData.CanData[1] = (byte)Angle;
            }
        }

        public AlgoArmRootServo(string name, string description, double angleMax, double angleMin, Algo algo) : base(name, description, Math.Max(angleMin, -127), Math.Min(angleMax, 127))
        {
            Parent = algo;
            Angle = 30;
            algo.SetLiftDefault += SetDefaultCanData;
        }
        public void SetDefaultCanData(LiftControlCanData data)
        {
            data.CanData[1] = (byte)Angle;
        }
        public override string ModuleName => "アルゴ アーム根本 モーター";

        public override string ModuleDescription => "アルゴの汎用アームの根本操作用サーボモーター";
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

    public class LiftControlCanData
    {
        CanID ID { get; } = new CanID()
        {
            DestinationAddress = (CanDestinationAddress)4,
            SourceAddress = CanDestinationAddress.BroadCast,
            MessageType = CanDataType.Command
        };
        public byte[] CanData { get; } = new byte[2];
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
