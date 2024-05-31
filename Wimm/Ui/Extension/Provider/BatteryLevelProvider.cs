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
            private IEnumerable<IBatteryLevelProvidable.Battery> batteries=Array.Empty<IBatteryLevelProvidable.Battery>();
            private IBatteryLevelProvidable Source { get; }
            public BatteryLevel(IBatteryLevelProvidable providable)
            {
                Source = providable;
                Update();
            }
            public void Update()
            {
                Batteries = Source.Batteries;
            }
            public IEnumerable<IBatteryLevelProvidable.Battery> Batteries
            {
                get { return batteries; }
                private set { batteries = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Batteries))); }
            }
            public event PropertyChangedEventHandler? PropertyChanged;
        }
    }
}
