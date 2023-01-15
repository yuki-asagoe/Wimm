using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Vortice.XInput;

namespace Wimm.Ui.ViewModel
{
    public class ControllerSettingViewModel:DependencyObject
    {
        public ObservableCollection<GameControllerModel> Gamepads { get; }
        public ControllerSettingViewModel()
        {

            Gamepads = new ObservableCollection<GameControllerModel>(
                new GameControllerModel[]
                {
                    new GameControllerModel(0),
                    new GameControllerModel(1),
                    new GameControllerModel(2),
                    new GameControllerModel(3)
                }
            );
        }
        public void UpdateState()
        {
            foreach(var i in Gamepads)
            {
                i.Update();
            }
        }
    }
    public class GameControllerModel:INotifyPropertyChanged
    {
        public GameControllerModel(int userIndex)
        {
            index = userIndex;
        }
        private int index;
        private bool connected;
        public bool Connected { get { return connected; } private set { connected = value; Notify(nameof(Connected)); } }
        public int Index { get { return index; } }
        private Gamepad gamepad;
        public Gamepad Gamepad { get { return gamepad; } private set { gamepad = value; Notify(nameof(Gamepad)); } }
        public BatteryInformation battery;
        public BatteryInformation Battery {
            get { return battery; } 
            private set {
                if (battery.Equals(value)) return;
                battery = value; 
                Notify(nameof(Battery));  
            } 
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void Notify(String propertyName = "")
        {
            if(PropertyChanged is not null)PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public void Update()
        {
            if (XInput.GetState(index, out var state))
            {
                Gamepad = state.Gamepad;
                Connected = true;
            }
            else
            {
                Connected = false;
            }
            if(XInput.GetBatteryInformation(index,BatteryDeviceType.Gamepad,out var info))
            {
                Battery = info;
            }
        }
    }
}
