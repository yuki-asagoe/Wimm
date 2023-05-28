using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Wimm.Ui.Converter
{
    internal class PercentageToBatteryLevelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is double percentage)
            {
                return percentage switch
                {
                    <=25 => PackIconModernKind.Battery0,
                    <=50 => PackIconModernKind.Battery1,
                    <=75 => PackIconModernKind.Battery2,
                    _ => PackIconModernKind.Battery3
                };
            }
            return PackIconModernKind.Question;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
