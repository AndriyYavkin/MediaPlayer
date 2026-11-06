using MediaPlayerApp.Models;

namespace MediaPlayerApp.Tests;

public class MediaItemTests
{
    [Theory]
    [InlineData("song.mp3", MediaType.Audio)]
    [InlineData("track.wav", MediaType.Audio)]
    [InlineData("voice.aac", MediaType.Audio)]
    [InlineData("sound.flac", MediaType.Audio)]
    [InlineData("music.wma", MediaType.Audio)]
    [InlineData("clip.ogg", MediaType.Audio)]
    [InlineData("movie.mp4", MediaType.Video)]
    [InlineData("video.avi", MediaType.Video)]
    [InlineData("film.mkv", MediaType.Video)]
    [InlineData("show.wmv", MediaType.Video)]
    [InlineData("recording.mov", MediaType.Video)]
    [InlineData("stream.webm", MediaType.Video)]
    [InlineData("document.pdf", MediaType.Unknown)]
    [InlineData("image.png", MediaType.Unknown)]
    [InlineData("", MediaType.Unknown)]
    public void DetectMediaType_ReturnsExpectedMediaType(string fileName, MediaType expected)
    {
        MediaType actual = MediaItem.DetectMediaType(fileName);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DetectMediaType_HandlesUrlQueryStringsCorrectly()
    {
        string url = "https://example.com/video.mp4?token=abc123xyz&expire=99999";
        MediaType mediaType = MediaItem.DetectMediaType(url);
        Assert.Equal(MediaType.Video, mediaType);
    }

    [Fact]
    public void Constructor_SetsPropertiesAccurately()
    {
        var uri = new Uri("https://example.com/media.mp3");
        var item = new MediaItem(uri, "Example Song", MediaType.Audio, "Remote Test");

        Assert.Equal(uri, item.Uri);
        Assert.Equal("Example Song", item.Title);
        Assert.Equal(MediaType.Audio, item.MediaType);
        Assert.Equal("Remote Test", item.SourceDescription);
    }

    [Fact]
    public void Constructor_FallsBackWhenTitleIsEmpty()
    {
        var uri = new Uri("https://example.com/stream");
        var item = new MediaItem(uri, "  ", MediaType.Unknown, "  ");

        Assert.Equal("Unknown Media", item.Title);
        Assert.Equal("https://example.com/stream", item.SourceDescription);
    }
}
