using System.Windows.Interop;
using Wimm.Machines.Audio;
using Wimm.Machines.Impl.Algo.Audio;
using Wimm.Machines.Tpip3;
using Wimm.Machines.Video;
using Wimm.Machines.Impl.Algo.Component;
using System.Collections.Immutable;
using Wimm.Machines.Component;
using System.Security.Cryptography.Xml;

namespace Wimm.Machines.Impl.Algo
{
    [LoadTarget]
    public sealed class Algo : Tpip3Machine
    {
        public ModuleGroup Wheels { get; }
        public ModuleGroup Arm { get; }
            
        public override string Name => "アルゴ";

        public override Speaker Speaker { get; }

        public override Microphone Microphone { get; }

        public override Camera Camera { get; }
        public AlgoControlProcess? ControlProcess{ get; set; }
        public override ControlProcess StartControlProcess()
        {
            ControlProcess = new AlgoControlProcess(() => { this.ControlProcess = null; });
            return ControlProcess;
        }
        private static ModuleGroup CreateWheels(Algo algo)
        {
            return new ModuleGroup(
                "wheels",
                ImmutableArray<ModuleGroup>.Empty,
                ImmutableArray.Create<Module>(
                    new AlgoWheelMotor(
                        "r_front_wheel", "機動用右前メカナムホイール",
                        WheelMotorDataIndex.RightFront, algo
                    ),
                    new AlgoWheelMotor(
                        "r_back_wheel","機動用右後メカナムホイール",
                        WheelMotorDataIndex.RightBack,algo
                    ),
                    new AlgoWheelMotor(
                        "l_front_wheel","機動用左前メカナムホイール",
                        WheelMotorDataIndex.LeftFront,algo
                    ),
                    new AlgoWheelMotor(
                        "l_back_wheel","機動用左後メカナムホイール",
                        WheelMotorDataIndex.LeftBack,algo
                    )
                )
            );
        }
        private static ModuleGroup CreateArm(Algo algo)
        {
            return new ModuleGroup(
                "arms",
                ImmutableArray<ModuleGroup>.Empty,
                ImmutableArray.Create<Module>(
                    new AlgoArmServo(
                        "roll_servo","アームのひねりを扱うサーボ",
                        ArmDataIndex.Roll,-80,80,algo
                    ),
                    new AlgoArmServo(
                        "pitch_servo", "アームの上下回転を扱うサーボ",
                        ArmDataIndex.Pitch, -85, 85, algo
                    ),
                    new AlgoArmServo(
                        "grip_servo", "アームのつかみを扱うサーボ",
                        ArmDataIndex.Grip, -60, 65, algo
                    ),
                    new AlgoArmServo(
                        "roll_servo", "アームのカメラの回転を扱うサーボ",
                        ArmDataIndex.Camera, -70, 25, algo
                    )
                )
            );
        }
        public Algo():base()
        {
            Microphone = new AlgoMicrophone();
            Speaker = new AlgoSpeaker();
            Camera = new Tpip3Camera(
                "フロント", "アーム", "バック", "トップ"
            );
            Wheels = CreateWheels(this);
            Arm = CreateArm(this);
            StructuredModules
                = new ModuleGroup(
                    "modules",
                    ImmutableArray.Create(Wheels, Arm),
                    ImmutableArray.Create<Module>(
                        new AlgoInnerContainer("container",this),
                        new OtherFeatureModule("others", "その他機能を提供するモジュール", this)
                    )
                );
        }
        public Algo(string tpipIpAddress,HwndSource hwnd) : base(tpipIpAddress, hwnd)
        {
            Microphone = new AlgoMicrophone();
            Speaker = new AlgoSpeaker();
            Camera = new Tpip3Camera(
                "フロント", "バック", "アーム","トップ"
            );
            if(Camera is Tpip3Camera camera)
            {
                hwnd.AddHook(camera.WndProc);
            }
            Wheels = CreateWheels(this);
            Arm = CreateArm(this);
            StructuredModules
                = new ModuleGroup(
                    "modules",
                    ImmutableArray.Create(Wheels,Arm),
                    ImmutableArray.Create<Module>(
                        new AlgoInnerContainer("container", this),
                        new OtherFeatureModule("others","その他機能を提供するモジュール",this)
                    )
                );
        }
    }
    public sealed class AlgoControlProcess : ControlProcess
    {
        Action FinishedHander { get; }
        public AlgoControlProcess(Action processFinishedHandler)
        {
            FinishedHander = processFinishedHandler;
        }
        public ContainerControlCanData ContainerData = new ContainerControlCanData();
        public ArmControlCanData ArmControlData { get; } = new ArmControlCanData();
        public WheelControlCanData WheelControlData { get; }= new WheelControlCanData();
        public override void Dispose()
        {
            ArmControlData.Send();
            WheelControlData.Send();
            ContainerData.Send();
            FinishedHander();
            base.Dispose();
        }
    }
}