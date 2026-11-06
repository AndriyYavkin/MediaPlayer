using MediaPlayerApp.Models;
using MediaPlayerApp.Services.Interfaces;

namespace MediaPlayerApp.Tests.Fakes;

/// <summary>
/// Fake file dialog service returning configurable file selection result.
/// </summary>
public sealed class FakeFileDialogService : IFileDialogService
{
    public string? FileToReturn { get; set; }

    public string? OpenMediaFile()
    {
        return FileToReturn;
    }
}

/// <summary>
/// Fake URL dialog service returning configurable remote stream URL.
/// </summary>
public sealed class FakeUrlInputDialogService : IUrlInputDialogService
{
    public string? UrlToReturn { get; set; }

    public string? PromptForMediaUrl(string? defaultUrl = null)
    {
        return UrlToReturn;
    }
}

/// <summary>
/// Fake message box service capturing error and information messages.
/// </summary>
public sealed class FakeMessageBoxService : IMessageBoxService
{
    public List<(string Message, string Title)> Errors { get; } = new();
    public List<(string Message, string Title)> Infos { get; } = new();

    public void ShowError(string message, string title)
    {
        Errors.Add((message, title));
    }

    public void ShowInfo(string message, string title)
    {
        Infos.Add((message, title));
    }
}

/// <summary>
/// Fake sample provider supplying mock sample media items.
/// </summary>
public sealed class FakeSampleProviderService : ISampleProviderService
{
    public List<MediaItem> Samples { get; } = new();

    public IReadOnlyList<MediaItem> GetSampleMediaItems()
    {
        return Samples.AsReadOnly();
    }
}
