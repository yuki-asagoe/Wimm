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
            private IEnumerable<IPowerVoltageProvidable.VoltageInfo> voltages=Array.Empty<IPowerVoltageProvidable.VoltageInfo>();
            private IPowerVoltageProvidable Source { get; }
            public PowerVoltage(IPowerVoltageProvidable providable)
            {
                Source = providable;
                Update();
            }
            public void Update()
            {
                Voltages = Source.Voltages;
            }
            public IEnumerable<IPowerVoltageProvidable.VoltageInfo> Voltages
            {
                get { return voltages; }
                set { voltages = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Voltages))); }
            }
            public double MaxVoltage { get; init; }
            public double MinVoltage { get; init; }
            public event PropertyChangedEventHandler? PropertyChanged;
        }
    }
}
