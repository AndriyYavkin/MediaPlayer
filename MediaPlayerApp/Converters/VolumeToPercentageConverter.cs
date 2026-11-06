using System.Globalization;
using System.Windows.Data;

namespace MediaPlayerApp.Converters;

/// <summary>
/// Converts double volume levels (0.0 - 1.0) to percentage strings.
/// </summary>
public sealed class VolumeToPercentageConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d)
        {
            int pct = (int)Math.Round(d * 100);
            return $"{pct}%";
        }

        return "0%";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
