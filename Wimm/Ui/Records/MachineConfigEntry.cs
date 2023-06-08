using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Wimm.Ui.Records
{
    public class MachineConfigEntry:INotifyPropertyChanged
    {
        private string name, value;
        private string? defaultValue;
        public string Name
        {
            get { return name; }
            set { name = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name))); }
        }
        public string Value
        {
            get { return this.value; }
            set { this.value = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value))); }
        }
        public string? Default
        {
            get { return defaultValue; }
            set { this.defaultValue = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Default))); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public MachineConfigEntry(string name,string value,string? defaultValue)
        {
            this.name = name;
            this.value = value;
            this.defaultValue = defaultValue;
        }
    }
}
