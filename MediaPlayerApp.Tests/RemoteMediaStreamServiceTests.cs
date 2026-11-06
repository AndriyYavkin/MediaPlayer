using MediaPlayerApp.Services.Implementations;

namespace MediaPlayerApp.Tests;

public class RemoteMediaStreamServiceTests
{
    [Fact]
    public async Task ResolveMediaUriAsync_ReturnsSameUri_WhenFileUri()
    {
        var service = new RemoteMediaStreamService();
        var fileUri = new Uri("C:/media/test.mp4");

        Uri result = await service.ResolveMediaUriAsync(fileUri);

        Assert.Equal(fileUri, result);
    }
}
