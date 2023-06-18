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
using Wimm.Logging;
using Wimm.Machines;
using Wimm.Model.Control.Script;
using Wimm.Model.Control.Script.Macro;
using Wimm.Ui.Records;

namespace Wimm.Model.Control
{
    public sealed class MachineController : IDisposable
    {
        private bool disposedValue;
        public event Action? OnStopMacro;
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
            ControlTimer = new Timer(OnTimer, null, Timeout.Infinite, Timeout.Infinite);
            ControlPeriod = controlPeriod;
            StopControlLoop();
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
                bool macroRunning = IsMacroRunning;
                MachineBridge.StartControlProcess();
                MachineBridge.DoControl();
                MachineBridge.EndControlProcess();
                if (macroRunning && !IsMacroRunning)
                    OnStopMacro?.Invoke();
            }
        }
        public void StartControlLoop()
        {
            ControlTimer.Change(0, ControlPeriod);
            Machine.Status = MachineState.Active;
            IsControlStopping = false;
        }
        public void StopControlLoop()
        {
            ControlTimer.Change(Timeout.Infinite, Timeout.Infinite);
            Machine.Status = MachineState.Idle;
            IsControlStopping = true;
        }
        public bool IsMacroRunning => MachineBridge.RunningMacro is not null;
        public ImmutableArray<MacroInfo> MacroList => MachineBridge.MacroList;
        public void StartMacro(MacroInfo macro)
        {
            lock(MachineBridge)
            {
                if (IsMacroRunning)
                {
                    MachineBridge.StopMacro();
                }
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
        public class Builder
        {
            public static MachineController Build(DirectoryInfo machineDirectory, MachineConstructorArgs? args, WimmFeatureProvider wimmFeature, ILogger? logger = null)
            {
                var dll = new FileInfo(machineDirectory + "/" + machineDirectory.Name + ".dll");
                Machine machine = GetMachine(dll, args);
                int gamepadIndex = GeneralSetting.Default.SelectedControllerIndex;
                ScriptDriver binder = new ScriptDriver(machine, machineDirectory, gamepadIndex, wimmFeature, logger);
                return new MachineController(machine, binder, gamepadIndex);
            }
            public static Machine GetMachine(FileInfo dll, MachineConstructorArgs? args)
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
                        $"Machineクラスの定義が見つかりませんでした。{nameof(LoadTargetAttribute)}が付与されているか等を確認してください。"
                    );
                }
                Machine? machine = null;
                if (args is not null)
                {
                    machine = GetMachineInstance(machineType, args);
                    if (machine is null)
                    {
                        throw new TypeLoadException(
                            $"型[{machineType.FullName}]に引数({nameof(MachineConstructorArgs)})のコンストラクタが見つかりませんでした。"
                        );
                    }
                }
                else
                {
                    machine = GetMachineInstance(machineType);
                    if (machine is null)
                    {
                        throw new TypeLoadException(
                            $"型[{machineType.FullName}]に引数なしのコンストラクタが見つかりませんでした。"
                        );
                    }
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
            public static Machine? GetMachineInstance(Type machineType)
            {
                if (machineType.IsSubclassOf(typeof(Machine)))
                {
                    var constructor = machineType.GetConstructor(Type.EmptyTypes);
                    return constructor?.Invoke(null) as Machine;
                }
                return null;
            }
            public static Machine? GetMachineInstance(Type machineType, MachineConstructorArgs args)
            {
                if (machineType.IsSubclassOf(typeof(Machine)))
                {
                    var constructor = machineType.GetConstructor(new Type[] { typeof(MachineConstructorArgs) });
                    return constructor?.Invoke(new[] { args }) as Machine;
                }
                return null;
            }
        }
    }
}
