using System.Globalization;

namespace PomodoroProject.Converters;

public class BoolToInverse : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
      !(bool)value;
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
      throw new NotSupportedException();
}