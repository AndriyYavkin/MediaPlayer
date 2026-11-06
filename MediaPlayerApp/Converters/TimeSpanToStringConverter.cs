using System.Globalization;
using System.Windows.Data;

namespace MediaPlayerApp.Converters;

/// <summary>
/// Converts TimeSpan values to formatted time strings (hh:mm:ss or mm:ss).
/// </summary>
public sealed class TimeSpanToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TimeSpan timeSpan)
        {
            if (timeSpan < TimeSpan.Zero)
            {
                timeSpan = TimeSpan.Zero;
            }

            if (timeSpan.TotalHours >= 1)
            {
                return timeSpan.ToString(@"hh\:mm\:ss", culture);
            }

            return timeSpan.ToString(@"mm\:ss", culture);
        }

        return "00:00";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
