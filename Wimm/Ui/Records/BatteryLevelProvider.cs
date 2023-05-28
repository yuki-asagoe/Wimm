using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wimm.Ui.Records
{
    public class BatteryLevelProvider : INotifyPropertyChanged
    {
        double percentage;
        public double Percentage
        {
            get { return percentage; }
            set { 
                percentage = value;
                PropertyChanged?.Invoke(
                    this,
                    new PropertyChangedEventArgs(nameof(Percentage))
                ); 
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
