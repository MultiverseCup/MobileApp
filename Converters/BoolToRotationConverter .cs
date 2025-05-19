using System.Globalization;

namespace PomodoroProject.Converters
{
    public class BoolToRotationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
          (bool)value ? 180 : 0;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
          throw new NotSupportedException();
    }
}