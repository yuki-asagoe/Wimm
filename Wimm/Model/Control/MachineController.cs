using Neo.IronLua;
using System;
using System.Collections.Immutable;
using System.Configuration.Internal;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using Wimm.Common;
using Wimm.Machines;
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
        public MachineController(Machine machine,ScriptDriver binder,int controllerIndex,int controlPeriod=50)
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
    }
}
