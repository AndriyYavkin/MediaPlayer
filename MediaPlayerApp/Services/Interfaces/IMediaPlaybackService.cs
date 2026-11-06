using MediaPlayerApp.Models;

namespace MediaPlayerApp.Services.Interfaces;

/// <summary>
/// Defines the contract for controlling media playback operations and monitoring state.
/// </summary>
public interface IMediaPlaybackService
{
    event EventHandler<PlaybackState>? StateChanged;
    event EventHandler<TimeSpan>? PositionChanged;
    event EventHandler<TimeSpan>? DurationChanged;
    event EventHandler<string>? PlaybackErrorOccurred;
    event EventHandler? MediaEnded;

    PlaybackState State { get; }
    TimeSpan CurrentPosition { get; }
    TimeSpan Duration { get; }
    double Volume { get; set; }
    bool IsMuted { get; set; }
    MediaItem? CurrentItem { get; }

    void Open(MediaItem item);
    void Play();
    void Pause();
    void Stop();
    void Seek(TimeSpan position);
    void StepForward(TimeSpan delta);
    void StepBackward(TimeSpan delta);
}
