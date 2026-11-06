using MediaPlayerApp.Models;

namespace MediaPlayerApp.Services.Interfaces;

/// <summary>
/// Defines the contract for supplying bundled sample media files.
/// </summary>
public interface ISampleProviderService
{
    IReadOnlyList<MediaItem> GetSampleMediaItems();
}
