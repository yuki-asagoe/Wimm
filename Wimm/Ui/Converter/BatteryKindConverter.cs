using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Vortice.XInput;

namespace Wimm.Ui.Converter
{
    public class BatteryKindConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BatteryInformation battery)
            {
                if (battery.BatteryType == BatteryType.Wired)
                    return PackIconModernKind.BatteryCharging;
                if (battery.BatteryType == BatteryType.Disconnected)
                    return PackIconModernKind.NetworkDisconnect;
                return battery.BatteryLevel switch
                {
                    BatteryLevel.Empty => PackIconModernKind.Battery0,
                    BatteryLevel.Low => PackIconModernKind.Battery1,
                    BatteryLevel.Medium => PackIconModernKind.Battery2,
                    BatteryLevel.Full => PackIconModernKind.Battery3,
                    _ => PackIconModernKind.Question
                };
            }
            else return PackIconModernKind.Question;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
