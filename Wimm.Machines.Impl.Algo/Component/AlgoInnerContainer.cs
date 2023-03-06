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
    public class AlgoInnerContainer : Module
    {
        Algo Parent { get; }
        public AlgoInnerContainer(string name,Algo parent) : base(name, CommonDescription)
        {
            Parent = parent;
            Features = ImmutableArray.Create<Feature<Delegate>>(
                new Feature<Delegate>(
                    "rotate_belt", "ベルトを回転させます。引数が正の値なら排出方向、負の値なら巻き込み方向",
                    RotateBelt
                ),
                new Feature<Delegate>(
                    "lift_updown", "コンテナを上下させます。引数が正の値なら持ち上げ、負の値ならおろす",
                    LiftContainer
                )
            );
        }
        /// <summary>
        /// ベルトの回転
        /// </summary>
        /// <param name="rotationDirection">正の値なら排出方向、負の値なら巻き込み方向</param>
        public void RotateBelt(int rotationDirection)
        {
            if(Parent.ControlProcess is AlgoControlProcess process)
            {
                process.WheelControlData.CanData[(int)ContainerDataIndex.Belt] = (byte)Math.Sign(rotationDirection);
            }
        }
        /// <summary>
        /// コンテナの上げ下げ
        /// </summary>
        /// <param name="liftDirection">正の値なら持ち上げ、負の値ならおろす</param>
        public void LiftContainer(int liftDirection)
        {
            if (Parent.ControlProcess is AlgoControlProcess process)
            {
                process.WheelControlData.CanData[(int)ContainerDataIndex.Bed] = (byte)Math.Sign(liftDirection);
            }
        }
        private const string CommonDescription = "アルゴ内部の救助対象格納用スペース";
        public override string ModuleName => "アルゴ 内部コンテナ";
        public override string ModuleDescription => CommonDescription;
    }
    public class ContainerControlCanData
    {
        CanID ID { get; } = new CanID()
        {
            DestinationAddress = (CanDestinationAddress)3,
            SourceAddress = CanDestinationAddress.BroadCast,
            MessageType = CanDataType.Command
        };
        public byte[] CanData { get; } = new byte[2];
        public void Send()
        {
            MotorBoardSupporter.Send(ID, CanData);
        }
    }
    public enum ContainerDataIndex
    {
        Bed=0,Belt
    }
}
