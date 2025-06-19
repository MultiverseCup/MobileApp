using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PomodoroProject.Converters;

class BoolToTextColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
      (bool)value ? Colors.White : Colors.Black;
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
      !(bool)value;

}
