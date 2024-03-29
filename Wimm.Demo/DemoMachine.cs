﻿using Wimm.Common;
using Wimm.Machines;

using System.Collections.Immutable;
using System.Diagnostics;

namespace Wimm.Demo.Machine
{
    [LoadTarget]
    public class DemoMachine : Machines.Machine
    {
        public DemoMachine(MachineConstructorArgs? args) : base(args)
        {
            Information = ImmutableArray.Create(
                new InformationNode("Demo",ImmutableArray<InformationNode>.Empty),
                new InformationNode("Informations",
                    ImmutableArray.Create(
                        new InformationNode("Time",ImmutableArray<InformationNode>.Empty),
                        new InformationNode("Config Test",ImmutableArray<InformationNode>.Empty)
                    )
                )
            );
            MachineConfig.AddRegistries(
                new Common.Setting.ConfigItemRegistry("Test","Default Value")
            );
        }
        public override void UpdateInformationTree()
        {
            // Informations/Time
            Information[1].Entries[0].Value = $"{Stopwatch.ElapsedMilliseconds} ms";
            // or Information[1]["Time"] (slightly slower)
            Information[1].Entries[1].Value = MachineConfig.GetValueOrDefault("Test")??string.Empty;
        }
        protected override void OnStatusChanged(MachineState newState)
        {
            switch (newState)
            {
                case MachineState.Idle:
                    Stopwatch.Stop();
                    break;
                case MachineState.Active:
                    Stopwatch.Start();
                    break;
            }
        }
        protected override ControlProcess StartControlProcess()
        {
            return base.StartControlProcess();
        }
        public override void Reset()
        {
            base.Reset();
            Stopwatch.Reset();
        }

        public override string Name => "Demo Machine";

        public override string ControlSystem => "Demo";

        public override ConnectionState ConnectionStatus 
        {
            get { return Status == MachineState.Active ? ConnectionState.Online : ConnectionState.Offline; }
        }

        private Stopwatch Stopwatch { get; } = new Stopwatch();
    }
}