using Neo.IronLua;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration.Internal;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using Wimm.Machines;
using Wimm.Machines.Tpip3;
using Wimm.Model.Control.Script;
using Wimm.Model.Control.Script.Macro;

namespace Wimm.Model.Control
{
    public sealed class MachineController : IDisposable
    {
        private bool disposedValue;

        public Machine Machine { get; init; }
        public int ObservedGamepadIndex { get; } = 0;
        //こいつはThreading.Timerで別スレッドから走るので一応排他制御いれます。
        //多分特にマクロ操作関係が危ないかと
        private ScriptDriver MachineBridge { get; init; }
        private Timer ControlTimer { get; set; }
        private int ControlPeriod { get; init; }
        public bool IsControlStopping { get; private set; }
        public double MacroProgress { get { lock (MachineBridge) { return MachineBridge.RunningMacro?.Progress ?? 0; } } }
        public double MacroRunningSecond
        {
            get
            {
                lock (MachineBridge)
                {
                    return MachineBridge.RunningMacro?.RunningSecond ?? 0;
                }
            }
        }
        public double MacroMaxSecond
        {
            get
            {
                lock (MachineBridge)
                {
                    return MachineBridge.RunningMacro?.MaxRunningSecond ?? 1;
                }
            }
        }
        private MachineController(Machine machine,ScriptDriver binder,int controllerIndex,int controlPeriod=50)
        {
            Machine = machine;
            MachineBridge = binder;
            ObservedGamepadIndex = controllerIndex;
            ControlTimer = new Timer(OnTimer, null, 0, controlPeriod);
            ControlPeriod = controlPeriod;
            if (Machine.Camera.Channels.Length > 0)
            {
                Machine.Camera.Activate(Machine.Camera.Channels[0], true);
            }
        }
        //この内部は別スレッド
        private void OnTimer(object? state)
        {
            Control();
        }
        private void Control()
        {
            lock (MachineBridge)
            {
                MachineBridge.StartControlProcess();
                MachineBridge.DoControl();
                MachineBridge.EndControlProcess();
            }
        }
        public class Builder
        {
            public static MachineController Build(DirectoryInfo machineDirectory,Tpip3ConstructorArgs? tpip3Arg)
            {
                var dll = new FileInfo(machineDirectory + "/" + machineDirectory.Name + ".dll");
                Machine machine = GetMachine(dll,tpip3Arg);
                int gamepadIndex = GeneralSetting.Default.SelectedControllerIndex;
                ScriptDriver binder = new ScriptDriver(machine,machineDirectory,gamepadIndex);
                return new MachineController(machine, binder,gamepadIndex);
            }
            public static Machine GetMachine(FileInfo dll,Tpip3ConstructorArgs? tpip3Arg)
            {
                if (!dll.Exists)
                {
                    throw new FileNotFoundException(
                        $"\"{dll}\"を検索しましたが、存在しません。"
                    );
                }
                var dllAssembly = Assembly.LoadFrom(dll.FullName);
                if (dllAssembly is null)
                {
                    throw new FileLoadException(
                        "アセンブリの読み込みに失敗しました。"
                    );
                }
                var machineType = GetMachineType(dllAssembly);
                if (machineType is null)
                {
                    throw new TypeLoadException(
                        "Machineクラスの定義が見つかりませんでした。"
                    );
                }
                Machine? machine =
                    tpip3Arg is not null ? GetTpip3Machine(machineType,tpip3Arg):null ??//TODO
                    GetDefaultMachine(machineType);
                if (machine is null)
                {
                    throw new TypeLoadException(
                        "Machineクラスのインスタンス化に失敗しました。"
                    );
                }
                return machine;
            }
            public static Type? GetMachineType(Assembly assembly)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (
                        type.IsSubclassOf(typeof(Machine)) &&
                        type.GetCustomAttribute<LoadTargetAttribute>() is not null
                    )
                    {
                        return type;
                    }
                }
                return null;
            }
            public static Machine? GetTpip3Machine(Type machineType, Tpip3ConstructorArgs args)
            {
                if (machineType.IsSubclassOf(typeof(Tpip3Machine)))
                {
                    var constructor = machineType.GetConstructor(new Type[] { typeof(string), typeof(HwndSource) });
                    return constructor?.Invoke(new object[] { args.TpipIpAddress, args.Hwnd }) as Tpip3Machine;
                }
                return null;
            }
            public static Machine? GetDefaultMachine(Type machineType)
            {
                if (machineType.IsSubclassOf(typeof(Machine)))
                {
                    var constructor = machineType.GetConstructor(Type.EmptyTypes);
                    return constructor?.Invoke(null) as Machine;
                }
                return null;
            }
        }
        public void StartControlLoop()
        {
            ControlTimer.Change(0, ControlPeriod);
            IsControlStopping = false;
        }
        public void StopControlLoop()
        {
            ControlTimer.Change(Timeout.Infinite, Timeout.Infinite);
            IsControlStopping = true;
        }
        public bool IsMacroRunning => MachineBridge.RunningMacro is not null;
        public ImmutableArray<MacroInfo> MacroList => MachineBridge.MacroList;
        public void StartMacro(MacroInfo macro)
        {
            lock(MachineBridge)
            {
                MachineBridge.StartMacro(macro);
            }
        }
        public void StopMacro()
        {
            lock (MachineBridge)
            {
                MachineBridge.StopMacro();
            }
        }
        public void Dispose()
        {
            if (!disposedValue)
            {
                MachineBridge.Dispose();
                ControlTimer.Dispose();
                Machine.Dispose();
                GC.SuppressFinalize(this);
                disposedValue = true;
            }
        }
        ~MachineController()
        {
            Dispose();
        }
    }
    public record class Tpip3ConstructorArgs(HwndSource Hwnd, string TpipIpAddress) { }
}
