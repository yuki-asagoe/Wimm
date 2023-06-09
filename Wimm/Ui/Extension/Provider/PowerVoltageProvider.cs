using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wimm.Machines.Extension;

namespace Wimm.Ui.Extension.Provider
{
    [ExtensionProvider(typeof(IPowerVoltageProvidable))]
    internal sealed class PowerVoltageProvider : ExtensionViewProvider
    {
        public PowerVoltageProvider(IMachineExtension instance) : base(instance)
        {
            Source = (IPowerVoltageProvidable)instance;
            DataContext = new PowerVoltage(Source);
            View = new View.PowerVoltageView
            {
                DataContext = this.DataContext
            };
        }
        private IPowerVoltageProvidable Source { get; }
        private PowerVoltage DataContext { get; }

        public override FrameworkElement View { get; init; }

        public override string Name => "Power Voltage";

        public override void OnPeriodicTimer()
        {
            DataContext.Update();
        }
        public class PowerVoltage : INotifyPropertyChanged
        {
            private double voltage;
            private IPowerVoltageProvidable Source { get; }
            public PowerVoltage(IPowerVoltageProvidable providable)
            {
                Source = providable;
                MaxVoltage = providable.MaxVoltage;
                MinVoltage = providable.MinVoltage;
                voltage = providable.Voltage;
            }
            public void Update()
            {
                Voltage = Source.Voltage;
            }
            public double Voltage
            {
                get { return voltage; }
                set { voltage = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Voltage))); }
            }
            public double MaxVoltage { get; init; }
            public double MinVoltage { get; init; }
            public event PropertyChangedEventHandler? PropertyChanged;
        }
    }
}
