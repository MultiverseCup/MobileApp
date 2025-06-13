using System.Globalization;

namespace PomodoroProject.Converters;

public class BoolToStrike : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
      (bool)value ? TextDecorations.Strikethrough : TextDecorations.None;
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
      throw new NotSupportedException();
}

public class BoolToGreenColor : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
      (bool)value ? Colors.Green : Colors.Black;
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
      throw new NotSupportedException();
}