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
using Wimm.Common;
using Wimm.Common.Logging;
using Wimm.Device;
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
        private ScriptDriver ScriptDriver { get; init; }
        private Timer ControlTimer { get; set; }
        private int ControlPeriod { get; init; }
        public bool IsControlStopping { get; private set; }
        public double MacroProgress { get { lock (ScriptDriver) { return ScriptDriver.RunningMacro?.Progress ?? 0; } } }
        public double MacroRunningSecond
        {
            get
            {
                lock (ScriptDriver)
                {
                    return ScriptDriver.RunningMacro?.RunningSecond ?? 0;
                }
            }
        }
        public double MacroMaxSecond
        {
            get
            {
                lock (ScriptDriver)
                {
                    return ScriptDriver.RunningMacro?.MaxRunningSecond ?? 1;
                }
            }
        }
        private MachineController(Machine machine,ScriptDriver binder,int controllerIndex,int controlPeriod=50)
        {
            Machine = machine;
            ScriptDriver = binder;
            ObservedGamepadIndex = controllerIndex;
            ControlTimer = new Timer(OnTimer, null, Timeout.Infinite, Timeout.Infinite);
            ControlPeriod = controlPeriod;
            StopControlLoop();
        }
        //この内部は別スレッド
        private void OnTimer(object? state)
        {
            Control();
            if (!IsControlStopping) ControlTimer.Change(ControlPeriod, Timeout.Infinite);
        }
        private void Control()
        {
            lock (ScriptDriver)
            {
                bool macroRunning = IsMacroRunning;
                ScriptDriver.StartControlProcess();
                ScriptDriver.DoControl();
                ScriptDriver.EndControlProcess();
                if (macroRunning && !IsMacroRunning)
                    OnStopMacro?.Invoke();
            }
        }
        public Task<(object? returnedValue, Exception? e)> RequestExcutionAsync(Feature<Delegate> feature,params object[]? args)
        {
            return Task.Run<(object?,Exception?)>(() =>
            {
                object? returned = null;
                try
                {
                    lock (ScriptDriver)
                    {
                        ScriptDriver.StartControlProcess();
                        returned = feature.Function.DynamicInvoke(args);
                        ScriptDriver.EndControlProcess();
                    }
                }
                catch (TargetInvocationException e)
                {
                    return (null, e);
                }
                catch (ArgumentException e) {
                    return (null, e);
                }
                catch (TargetParameterCountException e)
                {
                    return (null, e);
                }
                return (returned, null);
            });
        }
        public void StartControlLoop()
        {
            IsControlStopping = false;
            ControlTimer.Change(ControlPeriod,Timeout.Infinite);
            Machine.Status = MachineState.Active;
        }
        public void StopControlLoop()
        {
            IsControlStopping = true;
            ControlTimer.Change(Timeout.Infinite, Timeout.Infinite);
            Machine.Status = MachineState.Idle;
        }
        public bool IsMacroRunning => ScriptDriver.RunningMacro is not null;
        public ImmutableArray<MacroInfo> MacroList => ScriptDriver.MacroList;
        public void StartMacro(MacroInfo macro)
        {
            lock(ScriptDriver)
            {
                if (IsMacroRunning)
                {
                    ScriptDriver.StopMacro();
                }
                ScriptDriver.StartMacro(macro);
            }
        }
        public void StopMacro()
        {
            lock (ScriptDriver)
            {
                ScriptDriver.StopMacro();
            }
        }
        public Task<Exception?> CallScriptStringAsync(string s) => Task.Run(
            () =>
            {
                try
                {
                    ScriptDriver.ExecuteScriptString(s);
                }
                catch(Exception e)
                {
                    return e;
                }
                return null;
            }
        );
        public void Dispose()
        {
            if (!disposedValue)
            {
                ScriptDriver.Dispose();
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
            public static MachineController Build(MachineFolder machineDirectory, WimmFeatureProvider wimmFeature, IntPtr hwnd, ILogger logger)
            {
                var dll = machineDirectory.MachineAssemblyFile;
                var args = new MachineConstructorArgs(hwnd, new MachineLogger(logger), machineDirectory.MachineDirectory);
                Machine machine = GetMachine(dll, args);
                int gamepadIndex = GeneralSetting.Default.SelectedControllerIndex;
                ScriptDriver binder = new ScriptDriver(machine, machineDirectory.MachineDirectory, gamepadIndex, wimmFeature, hwnd, logger);
                int contorlInterval = GeneralSetting.Default.ControlInterval;
                return new MachineController(machine, binder, gamepadIndex,contorlInterval);
            }
            public static MachineController Build(DirectoryInfo machineDirectory,WimmFeatureProvider wimmFeature, IntPtr hwnd, ILogger logger)
            {
                return Build(new MachineFolder(machineDirectory), wimmFeature, hwnd, logger);
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
                machine = GetMachineInstance(machineType, args);
                if (machine is null)
                {
                    throw new TypeLoadException(
                        $"型[{machineType.FullName}]に引数({nameof(MachineConstructorArgs)})のコンストラクタが見つかりませんでした。"
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
            public static Machine? GetMachineInstance(Type machineType)
            {
                if (machineType.IsSubclassOf(typeof(Machine)))
                {
                    var constructor = machineType.GetConstructor(Type.EmptyTypes);
                    return constructor?.Invoke(null) as Machine;
                }
                return null;
            }
            public static Machine? GetMachineInstance(Type machineType, MachineConstructorArgs? args)
            {
                if (machineType.IsSubclassOf(typeof(Machine)))
                {
                    var constructor = machineType.GetConstructor(new Type[] { typeof(MachineConstructorArgs) });
                    return constructor?.Invoke(new[] { args }) as Machine;
                }
                return null;
            }
            public static IODevice GetDevice(FileInfo dll,DeviceConstructorArgs? args)
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
                var deviceType = GetDeviceType(dllAssembly);
                if (deviceType is null)
                {
                    throw new TypeLoadException(
                        $"IODeviceクラスの定義が見つかりませんでした。{nameof(LoadTargetAttribute)}が付与されているか等を確認してください。"
                    );
                }
                IODevice? device = GetDeviceInstance(deviceType, args);
                if (device is null)
                {
                    throw new TypeLoadException(
                        $"型[{deviceType.FullName}]に引数({nameof(MachineConstructorArgs)}?)のコンストラクタが見つかりませんでした。"
                    );
                }
                return device;
            }
            public static Type? GetDeviceType(Assembly assembly)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (
                        type.IsSubclassOf(typeof(IODevice)) &&
                        type.GetCustomAttribute<LoadTargetAttribute>() is not null
                    )
                    {
                        return type;
                    }
                }
                return null;
            }
            public static IODevice? GetDeviceInstance(Type type, DeviceConstructorArgs? args)
            {
                if (type.IsSubclassOf(typeof(IODevice)))
                {
                    var constructor = type.GetConstructor(new Type[] {typeof(DeviceConstructorArgs)});
                    return constructor?.Invoke(new[] {args}) as IODevice;
                }
                return null;
            }
        }
    }
}
