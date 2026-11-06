using System.Windows.Controls;
using System.Windows.Threading;
using MediaPlayerApp.Models;
using MediaPlayerApp.Services.Interfaces;

namespace MediaPlayerApp.Services.Implementations;

/// <summary>
/// Implements media playback operations using WPF MediaElement and DispatcherTimer.
/// </summary>
public sealed class WpfMediaPlaybackService : IMediaPlaybackService, IMediaPlayerHost
{
    private readonly IRemoteMediaStreamService _remoteMediaStreamService;
    private MediaElement? _mediaElement;
    private readonly DispatcherTimer _positionTimer;
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

    public PlaybackState State
    {
        get => _state;
        private set
        {
            if (_state != value)
            {
                _state = value;
                StateChanged?.Invoke(this, _state);
            }
        }
    }

    public TimeSpan CurrentPosition
    {
        get => _currentPosition;
        private set
        {
            if (_currentPosition != value)
            {
                _currentPosition = value;
                PositionChanged?.Invoke(this, _currentPosition);
            }
        }
    }

    public TimeSpan Duration
    {
        get => _duration;
        private set
        {
            if (_duration != value)
            {
                _duration = value;
                DurationChanged?.Invoke(this, _duration);
            }
        }
    }

    public double Volume
    {
        get => _volume;
        set
        {
            double clamped = Math.Clamp(value, 0.0, 1.0);
            if (Math.Abs(_volume - clamped) > 0.0001)
            {
                _volume = clamped;
                if (_mediaElement != null)
                {
                    _mediaElement.Volume = _volume;
                }
            }
        }
    }

    public bool IsMuted
    {
        get => _isMuted;
        set
        {
            if (_isMuted != value)
            {
                _isMuted = value;
                if (_mediaElement != null)
                {
                    _mediaElement.IsMuted = _isMuted;
                }
            }
        }
    }

    public MediaItem? CurrentItem => _currentItem;

    public WpfMediaPlaybackService(IRemoteMediaStreamService remoteMediaStreamService)
    {
        _remoteMediaStreamService = remoteMediaStreamService ?? throw new ArgumentNullException(nameof(remoteMediaStreamService));

        _positionTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(150)
        };
        _positionTimer.Tick += OnPositionTimerTick;
    }

    public void AttachMediaElement(MediaElement mediaElement)
    {
        if (_mediaElement != null)
        {
            _mediaElement.MediaOpened -= OnMediaOpened;
            _mediaElement.MediaEnded -= OnMediaEnded;
            _mediaElement.MediaFailed -= OnMediaFailed;
            _mediaElement.BufferingStarted -= OnBufferingStarted;
            _mediaElement.BufferingEnded -= OnBufferingEnded;
        }

        _mediaElement = mediaElement ?? throw new ArgumentNullException(nameof(mediaElement));
        _mediaElement.LoadedBehavior = MediaState.Manual;
        _mediaElement.UnloadedBehavior = MediaState.Manual;
        _mediaElement.Volume = _volume;
        _mediaElement.IsMuted = _isMuted;

        _mediaElement.MediaOpened += OnMediaOpened;
        _mediaElement.MediaEnded += OnMediaEnded;
        _mediaElement.MediaFailed += OnMediaFailed;
        _mediaElement.BufferingStarted += OnBufferingStarted;
        _mediaElement.BufferingEnded += OnBufferingEnded;
    }

    public async void Open(MediaItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        _currentItem = item;
        CurrentPosition = TimeSpan.Zero;
        Duration = TimeSpan.Zero;

        if (_mediaElement == null)
        {
            return;
        }

        _mediaElement.Stop();
        _positionTimer.Stop();

        if (item.Uri.Scheme == Uri.UriSchemeHttp || item.Uri.Scheme == Uri.UriSchemeHttps)
        {
            State = PlaybackState.Buffering;
            try
            {
                Uri resolvedUri = await _remoteMediaStreamService.ResolveMediaUriAsync(item.Uri);
                if (_currentItem != item)
                {
                    return;
                }

                _mediaElement.Source = resolvedUri;
                _mediaElement.Play();
                State = PlaybackState.Playing;
                _positionTimer.Start();
            }
            catch (Exception ex)
            {
                State = PlaybackState.Stopped;
                PlaybackErrorOccurred?.Invoke(this, "Failed to load remote media stream: " + ex.Message);
            }
        }
        else
        {
            _mediaElement.Source = item.Uri;
            _mediaElement.Play();
            State = PlaybackState.Playing;
            _positionTimer.Start();
        }
    }

    public void Play()
    {
        if (_mediaElement == null || _currentItem == null)
        {
            return;
        }

        _mediaElement.Play();
        State = PlaybackState.Playing;
        _positionTimer.Start();
    }

    public void Pause()
    {
        if (_mediaElement == null)
        {
            return;
        }

        _mediaElement.Pause();
        State = PlaybackState.Paused;
        _positionTimer.Stop();
    }

    public void Stop()
    {
        if (_mediaElement == null)
        {
            return;
        }

        _mediaElement.Stop();
        _positionTimer.Stop();
        CurrentPosition = TimeSpan.Zero;
        State = PlaybackState.Stopped;
    }

    public void Seek(TimeSpan position)
    {
        if (_mediaElement == null)
        {
            return;
        }

        TimeSpan target = position;
        if (target < TimeSpan.Zero)
        {
            target = TimeSpan.Zero;
        }
        else if (Duration > TimeSpan.Zero && target > Duration)
        {
            target = Duration;
        }

        _mediaElement.Position = target;
        CurrentPosition = target;
    }

    public void StepForward(TimeSpan delta)
    {
        Seek(CurrentPosition + delta);
    }

    public void StepBackward(TimeSpan delta)
    {
        Seek(CurrentPosition - delta);
    }

    private void OnPositionTimerTick(object? sender, EventArgs e)
    {
        if (_mediaElement != null && State == PlaybackState.Playing)
        {
            CurrentPosition = _mediaElement.Position;
        }
    }

    private void OnMediaOpened(object? sender, System.Windows.RoutedEventArgs e)
    {
        if (_mediaElement != null && _mediaElement.NaturalDuration.HasTimeSpan)
        {
            Duration = _mediaElement.NaturalDuration.TimeSpan;
        }
        else
        {
            Duration = TimeSpan.Zero;
        }
    }

    private void OnMediaEnded(object? sender, System.Windows.RoutedEventArgs e)
    {
        _positionTimer.Stop();
        CurrentPosition = Duration;
        State = PlaybackState.Stopped;
        MediaEnded?.Invoke(this, EventArgs.Empty);
    }

    private void OnMediaFailed(object? sender, System.Windows.ExceptionRoutedEventArgs e)
    {
        _positionTimer.Stop();
        State = PlaybackState.Stopped;
        string errorMessage = e.ErrorException?.Message ?? "An error occurred while attempting to play the media stream.";
        PlaybackErrorOccurred?.Invoke(this, errorMessage);
    }

    private void OnBufferingStarted(object? sender, System.Windows.RoutedEventArgs e)
    {
        State = PlaybackState.Buffering;
    }

    private void OnBufferingEnded(object? sender, System.Windows.RoutedEventArgs e)
    {
        if (_mediaElement != null && State == PlaybackState.Buffering)
        {
            State = PlaybackState.Playing;
        }
    }
}
