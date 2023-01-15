using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Wimm.Machines.Component;
using Wimm.Machines.Impl.Algo.Component.RokkoOroshiMotorBoard;

namespace Wimm.Machines.Impl.Algo.Component
{
    public class AlgoWheelMotor : Motor
    {
        
        readonly WheelMotorDataIndex number;
        Algo Parent { get; }
        public AlgoWheelMotor(string name, string description,WheelMotorDataIndex number,Algo parent) : base(name, description)
        {
            this.number = number;
            Parent = parent;
        }
        public override Feature<Action<double>> RotationFeature => new Feature<Action<double>>(
            Motor.RotationFeatureName,"モータを回転させます。回転方向はモーターボードの実装依存です。",Rotation
        );
        void Rotation(double speed)
        {
            if(Parent.ControlProcess is AlgoControlProcess process)
            {
                speed = Math.Clamp(speed,-1,1);
                process.WheelControlData.CanData[(int)number]=(byte)(127 * speed*Parent.SpeedModifier);
            }
        }
        public override string ModuleName => "アルゴ メカナムホイール モータ";

        public override string ModuleDescription => "アルゴの機動用メカナムホイールを回転させるモーター";
    }
    public class WheelControlCanData
    {
        CanID ID { get; } = new CanID() {
            DestinationAddress=(CanDestinationAddress)2,
            SourceAddress=CanDestinationAddress.BroadCast,
            MessageType=CanDataType.Command
        };
        public byte[] CanData { get; } = new byte[4];
        public void Send()
        {
            MotorBoardSupporter.Send(ID, CanData);
        }
    }
    public enum WheelMotorDataIndex
    {
        RightFront=0,RightBack,LeftFront,LeftBack
    }
}
