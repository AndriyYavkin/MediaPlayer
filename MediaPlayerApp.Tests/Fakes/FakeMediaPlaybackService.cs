using MediaPlayerApp.Models;
using MediaPlayerApp.Services.Interfaces;

namespace MediaPlayerApp.Tests.Fakes;

/// <summary>
/// A fake media playback service for testing ViewModels without requiring WPF MediaElement.
/// </summary>
public sealed class FakeMediaPlaybackService : IMediaPlaybackService
{
    private PlaybackState _state = PlaybackState.Stopped;
    private TimeSpan _currentPosition = TimeSpan.Zero;
    private TimeSpan _duration = TimeSpan.Zero;
    private double _volume = 0.8;
    private bool _isMuted;
    private MediaItem? _currentItem;

    public event EventHandler<PlaybackState>? StateChanged;
    public event EventHandler<TimeSpan>? PositionChanged;
    public event EventHandler<TimeSpan>? DurationChanged;
    public event EventHandler<string>? PlaybackErrorOccurred;
    public event EventHandler? MediaEnded;

    public bool PlayCalled { get; private set; }
    public bool PauseCalled { get; private set; }
    public bool StopCalled { get; private set; }
    public TimeSpan? LastSeekedPosition { get; private set; }
    public TimeSpan? LastSteppedForward { get; private set; }
    public TimeSpan? LastSteppedBackward { get; private set; }

    public PlaybackState State
    {
        get => _state;
        set
        {
            _state = value;
            StateChanged?.Invoke(this, _state);
        }
    }

    public TimeSpan CurrentPosition
    {
        get => _currentPosition;
        set
        {
            _currentPosition = value;
            PositionChanged?.Invoke(this, _currentPosition);
        }
    }

    public TimeSpan Duration
    {
        get => _duration;
        set
        {
            _duration = value;
            DurationChanged?.Invoke(this, _duration);
        }
    }

    public double Volume
    {
        get => _volume;
        set => _volume = Math.Clamp(value, 0.0, 1.0);
    }

    public bool IsMuted
    {
        get => _isMuted;
        set => _isMuted = value;
    }

    public MediaItem? CurrentItem => _currentItem;

    public void Open(MediaItem item)
    {
        _currentItem = item;
        State = PlaybackState.Playing;
    }

    public void Play()
    {
        PlayCalled = true;
        State = PlaybackState.Playing;
    }

    public void Pause()
    {
        PauseCalled = true;
        State = PlaybackState.Paused;
    }

    public void Stop()
    {
        StopCalled = true;
        CurrentPosition = TimeSpan.Zero;
        State = PlaybackState.Stopped;
    }

    public void Seek(TimeSpan position)
    {
        LastSeekedPosition = position;
        CurrentPosition = position;
    }

    public void StepForward(TimeSpan delta)
    {
        LastSteppedForward = delta;
        Seek(CurrentPosition + delta);
    }

    public void StepBackward(TimeSpan delta)
    {
        LastSteppedBackward = delta;
        Seek(CurrentPosition - delta);
    }

    public void RaiseError(string message)
    {
        PlaybackErrorOccurred?.Invoke(this, message);
    }

    public void RaiseMediaEnded()
    {
        MediaEnded?.Invoke(this, EventArgs.Empty);
    }
}
