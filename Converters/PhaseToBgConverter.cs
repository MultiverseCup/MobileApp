using System.Globalization;

namespace PomodoroProject.Converters;

public class PhaseToBgConverter : IValueConverter
{

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
      (bool)value ? "bg.jpg" : "bg1_alt.png";
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
      throw new NotSupportedException();
}