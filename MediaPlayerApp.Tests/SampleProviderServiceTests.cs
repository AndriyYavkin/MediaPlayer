using MediaPlayerApp.Models;
using MediaPlayerApp.Services.Implementations;

namespace MediaPlayerApp.Tests;

public class SampleProviderServiceTests
{
    [Fact]
    public void GetSampleMediaItems_ReturnsAvailableSamples()
    {
        var service = new SampleProviderService();
        IReadOnlyList<MediaItem> items = service.GetSampleMediaItems();

        Assert.NotNull(items);
        Assert.True(items.Count >= 4);

        Assert.Contains(items, item => item.Uri.OriginalString.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) && item.MediaType == MediaType.Audio);
        Assert.Contains(items, item => item.Uri.OriginalString.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) && item.MediaType == MediaType.Audio);
        Assert.Contains(items, item => item.Uri.OriginalString.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) && item.MediaType == MediaType.Video);
        Assert.Contains(items, item => item.Uri.OriginalString.EndsWith(".avi", StringComparison.OrdinalIgnoreCase) && item.MediaType == MediaType.Video);

        Assert.Contains(items, item => item.Title.Contains("Beach", StringComparison.OrdinalIgnoreCase));
    }
}
