using System.Globalization;
using System.Windows;
using MediaPlayerApp.Converters;

namespace MediaPlayerApp.Tests;

public class ConvertersTests
{
    [Theory]
    [InlineData(0, "00:00")]
    [InlineData(45, "00:45")]
    [InlineData(75, "01:15")]
    [InlineData(3599, "59:59")]
    [InlineData(3600, "01:00:00")]
    [InlineData(3665, "01:01:05")]
    public void TimeSpanToStringConverter_FormatsCorrectly(double totalSeconds, string expected)
    {
        var converter = new TimeSpanToStringConverter();
        var timeSpan = TimeSpan.FromSeconds(totalSeconds);

        object result = converter.Convert(timeSpan, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0.0, "0%")]
    [InlineData(0.5, "50%")]
    [InlineData(0.85, "85%")]
    [InlineData(1.0, "100%")]
    public void VolumeToPercentageConverter_FormatsPercentageCorrectly(double volume, string expected)
    {
        var converter = new VolumeToPercentageConverter();

        object result = converter.Convert(volume, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void BooleanToVisibilityConverter_ConvertsStandardAndInverted()
    {
        var converter = new BooleanToVisibilityConverter();

        Assert.Equal(Visibility.Visible, converter.Convert(true, typeof(Visibility), null, CultureInfo.InvariantCulture));
        Assert.Equal(Visibility.Collapsed, converter.Convert(false, typeof(Visibility), null, CultureInfo.InvariantCulture));

        Assert.Equal(Visibility.Collapsed, converter.Convert(true, typeof(Visibility), "Invert", CultureInfo.InvariantCulture));
        Assert.Equal(Visibility.Visible, converter.Convert(false, typeof(Visibility), "Invert", CultureInfo.InvariantCulture));
    }
}
