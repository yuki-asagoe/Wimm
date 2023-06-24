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
        public static readonly KeyValuePair<string, Type>[] LuaChunkParams = new[]
        {
            new KeyValuePair<string,Type>("time",typeof(double)),
            new KeyValuePair<string,Type>("timein",typeof(Func<double,double,bool>)),
        };
        public RunningMacro StartMacro() => new RunningMacro(this);
    }
    public sealed class RunningMacro : IDisposable
    {
        MacroInfo Info { get; init; }
        Stopwatch Timer { get; } = new Stopwatch();
        public double RunningSecond => Timer.ElapsedMilliseconds / 1000d;
        public double MaxRunningSecond => Info.LifeTimeSeconds;
        public double Progress => RunningSecond / MaxRunningSecond;
        public RunningMacro(MacroInfo info)
        {
            Info = info;
            Timer.Start();
        }
        public void Process(LuaGlobal environment)
        {
            double milisecond = Timer.ElapsedMilliseconds;
            if (Info.LifeTimeSeconds*1000 < milisecond)
            {
                Timer.Stop();
                return;
            }
            environment.DoChunk(
                Info.Chunk,
                milisecond,
                (double start, double until) => start <= milisecond && milisecond < until
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
