using System.Globalization;

namespace PomodoroProject.Converters;

public class BoolToWeekdayClrConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
      (bool)value ? Color.FromArgb("FF7E7E") : Colors.White;
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
      throw new NotSupportedException();
}