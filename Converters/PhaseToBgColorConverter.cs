using System.Globalization;

namespace PomodoroProject.Converters;

public class PhaseToBgColorConverter : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
      (bool)value ? "#FF7E7E" : "#7E8AFF";
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
      throw new NotSupportedException();
}