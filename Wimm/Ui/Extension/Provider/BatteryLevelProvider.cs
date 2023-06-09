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
    [ExtensionProvider(typeof(IBatteryLevelProvidable))]
    internal class BatteryLevelProvider : ExtensionViewProvider
    {
        public BatteryLevelProvider(IMachineExtension instance) : base(instance)
        {
            Source = (IBatteryLevelProvidable)instance;
            DataContext = new BatteryLevel(Source);
            View = new View.BatteryLevelView()
            {
                DataContext = this.DataContext
            };
        }
        private IBatteryLevelProvidable Source { get; }
        private BatteryLevel DataContext { get; }
        public override FrameworkElement View { get; init; }

        public override string Name => "Battery Level";

        public override void OnPeriodicTimer()
        {
            DataContext.Update();
        }
        public class BatteryLevel : INotifyPropertyChanged
        {
            private bool charging;
            private double percentage;
            private IBatteryLevelProvidable Source { get; }
            public BatteryLevel(IBatteryLevelProvidable providable)
            {
                Source = providable;
            }
            public void Update()
            {
                Charging = Source.Charging;
                Percentage = Source.BatteryPercentage;
            }
            public bool Charging
            {
                get { return charging; }
                set { charging = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Charging))); }
            }
            public double Percentage
            {
                get { return percentage; }
                set { percentage = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Percentage))); }
            }
            public event PropertyChangedEventHandler? PropertyChanged;
        }
    }
}
