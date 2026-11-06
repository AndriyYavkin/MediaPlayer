using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MediaPlayerApp.Converters;

/// <summary>
/// Converts boolean values to Visibility with optional inversion via parameter.
/// </summary>
public sealed class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool flag = value is true;

        if (parameter is string paramStr && string.Equals(paramStr, "Invert", StringComparison.OrdinalIgnoreCase))
        {
            flag = !flag;
        }

        return flag ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            bool flag = visibility == Visibility.Visible;
            if (parameter is string paramStr && string.Equals(paramStr, "Invert", StringComparison.OrdinalIgnoreCase))
            {
                flag = !flag;
            }
            return flag;
        }

        return false;
    }
}
