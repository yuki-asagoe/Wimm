using Neo.IronLua;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wimm.Model.Control.Script.Macro
{
    public record MacroInfo(string Name, double LifeTimeSeconds, LuaChunk Chunk)
    {
        public RunningMacro StartMacro() => new RunningMacro(this);
    }
    public class RunningMacro : IDisposable
    {
        MacroInfo Info { get; init; }
        Stopwatch Timer { get; } = new Stopwatch();
        public double RunningSecond => Timer.ElapsedMilliseconds / 1000;
        public double MaxRunningSecond => Info.LifeTimeSeconds;
        public double Progress => RunningSecond / MaxRunningSecond;
        public RunningMacro(MacroInfo info)
        {
            Info = info;
        }
        public void Process(LuaGlobal environment)
        {
            double milisecond = Timer.ElapsedMilliseconds / 1000d;
            if (Info.LifeTimeSeconds < milisecond)
            {
                Timer.Stop();
                return;
            }
            environment.DoChunk(
                Info.Chunk,
                new KeyValuePair<string, object>("time", milisecond),
                new KeyValuePair<string, object>("timein",
                    (double start, double until) => start <= milisecond && milisecond < until
                )
            );
        }
        public bool IsRunning
        {
            get { return Timer.ElapsedMilliseconds / 1000d < Info.LifeTimeSeconds; }
        }

        public void Dispose()
        {
            Timer.Stop();
        }
    }
}
