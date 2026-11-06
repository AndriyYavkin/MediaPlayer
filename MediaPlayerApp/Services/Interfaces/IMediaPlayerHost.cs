using System.Windows.Controls;

namespace MediaPlayerApp.Services.Interfaces;

/// <summary>
/// Allows the user interface host to bind a visual MediaElement to the playback service.
/// </summary>
public interface IMediaPlayerHost
{
    void AttachMediaElement(MediaElement mediaElement);
}
