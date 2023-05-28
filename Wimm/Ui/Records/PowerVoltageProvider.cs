using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Wimm.Ui.Records
{
    public class PowerVoltageProvider : INotifyPropertyChanged
    {
        double max, min, current;
        public double MaxVoltage
        {
            get { return max; }
            set
            {
                max = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MaxVoltage)));
            }
        }
        public double MinVoltage
        {
            get { return min; }
            set
            {
                min = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MinVoltage)));
            }
        }
        public double CurrentVoltage
        {
            get { return current; }
            set
            {
                current = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentVoltage)));
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
