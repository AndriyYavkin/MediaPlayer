using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using MediaPlayerApp.Common;
using MediaPlayerApp.Models;
using MediaPlayerApp.Services.Interfaces;

namespace MediaPlayerApp.ViewModels;

/// <summary>
/// Primary ViewModel orchestrating playback commands, status presentation, and external service interactions.
/// </summary>
public sealed class MainViewModel : ObservableObject
{
    private readonly IMediaPlaybackService _playbackService;
    private readonly IFileDialogService _fileDialogService;
    private readonly IUrlInputDialogService _urlInputDialogService;
    private readonly IMessageBoxService _messageBoxService;
    private readonly ISampleProviderService _sampleProviderService;

    private MediaItem? _currentMediaItem;
    private PlaybackState _playbackState = PlaybackState.Stopped;
    private TimeSpan _currentPosition = TimeSpan.Zero;
    private TimeSpan _duration = TimeSpan.Zero;
    private double _progressValue;
    private double _maxProgressValue = 100.0;
    private string _currentTimeDisplay = "00:00";
    private string _totalTimeDisplay = "00:00";
    private double _volume = 0.8;
    private bool _isMuted;
    private bool _isUserDraggingProgress;
    private bool _isFullscreen;
    private int _currentPlaylistIndex = -1;

    public ObservableCollection<MediaItem> Playlist { get; } = new();

    public int CurrentPlaylistIndex
    {
        get => _currentPlaylistIndex;
        set => SetProperty(ref _currentPlaylistIndex, value);
    }

    public MediaItem? CurrentMediaItem
    {
        get => _currentMediaItem;
        private set
        {
            if (SetProperty(ref _currentMediaItem, value))
            {
                OnPropertyChanged(nameof(HasMedia));
                OnPropertyChanged(nameof(IsVideoVisible));
                OnPropertyChanged(nameof(IsAudioPlaceholderVisible));
                OnPropertyChanged(nameof(MediaTitleDisplay));
                OnPropertyChanged(nameof(MediaSubtitleDisplay));
            }
        }
    }

    public PlaybackState PlaybackState
    {
        get => _playbackState;
        private set
        {
            if (SetProperty(ref _playbackState, value))
            {
                OnPropertyChanged(nameof(IsPlaying));
                OnPropertyChanged(nameof(StatusBadgeText));
            }
        }
    }

    public TimeSpan CurrentPosition
    {
        get => _currentPosition;
        private set
        {
            if (SetProperty(ref _currentPosition, value))
            {
                if (!_isUserDraggingProgress)
                {
                    CurrentTimeDisplay = FormatTimeSpan(_currentPosition);
                    _progressValue = _currentPosition.TotalSeconds;
                    OnPropertyChanged(nameof(ProgressValue));
                }
            }
        }
    }

    public TimeSpan Duration
    {
        get => _duration;
        private set
        {
            if (SetProperty(ref _duration, value))
            {
                TotalTimeDisplay = FormatTimeSpan(_duration);
                MaxProgressValue = _duration.TotalSeconds > 0 ? _duration.TotalSeconds : 100.0;
            }
        }
    }

    public double ProgressValue
    {
        get => _progressValue;
        set
        {
            if (SetProperty(ref _progressValue, value))
            {
                if (_isUserDraggingProgress)
                {
                    CurrentTimeDisplay = FormatTimeSpan(TimeSpan.FromSeconds(_progressValue));
                }
            }
        }
    }

    public double MaxProgressValue
    {
        get => _maxProgressValue;
        private set => SetProperty(ref _maxProgressValue, value);
    }

    public string CurrentTimeDisplay
    {
        get => _currentTimeDisplay;
        private set => SetProperty(ref _currentTimeDisplay, value);
    }

    public string TotalTimeDisplay
    {
        get => _totalTimeDisplay;
        private set => SetProperty(ref _totalTimeDisplay, value);
    }

    public double Volume
    {
        get => _volume;
        set
        {
            if (SetProperty(ref _volume, Math.Clamp(value, 0.0, 1.0)))
            {
                _playbackService.Volume = _volume;
                if (_isMuted && _volume > 0.0)
                {
                    IsMuted = false;
                }
            }
        }
    }

    public bool IsMuted
    {
        get => _isMuted;
        set
        {
            if (SetProperty(ref _isMuted, value))
            {
                _playbackService.IsMuted = _isMuted;
            }
        }
    }

    public bool IsFullscreen
    {
        get => _isFullscreen;
        set => SetProperty(ref _isFullscreen, value);
    }

    public bool HasMedia => _currentMediaItem != null;

    public bool IsPlaying => PlaybackState == PlaybackState.Playing;

    public bool IsVideoVisible => _currentMediaItem != null && _currentMediaItem.MediaType == MediaType.Video;

    public bool IsAudioPlaceholderVisible => _currentMediaItem != null && _currentMediaItem.MediaType != MediaType.Video;

    public string MediaTitleDisplay => _currentMediaItem?.Title ?? "No Media Loaded";

    public string MediaSubtitleDisplay => _currentMediaItem?.SourceDescription ?? "Select a local file or stream URL to begin playback";

    public string StatusBadgeText => PlaybackState switch
    {
        PlaybackState.Playing => "Playing",
        PlaybackState.Paused => "Paused",
        PlaybackState.Buffering => "Buffering...",
        _ => HasMedia ? "Stopped" : "Ready"
    };

    public IReadOnlyList<MediaItem> SampleItems { get; }

    public ICommand OpenFileCommand { get; }
    public ICommand OpenUrlCommand { get; }
    public ICommand PlayPauseCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand PlayNextCommand { get; }
    public ICommand PlayPreviousCommand { get; }
    public ICommand StepForwardCommand { get; }
    public ICommand StepBackwardCommand { get; }
    public ICommand JumpForwardCommand { get; }
    public ICommand JumpBackwardCommand { get; }
    public ICommand ToggleMuteCommand { get; }
    public ICommand BeginSeekCommand { get; }
    public ICommand EndSeekCommand { get; }
    public ICommand CommitDirectSeekCommand { get; }
    public ICommand LoadSampleCommand { get; }
    public ICommand ToggleFullscreenCommand { get; }

    public MainViewModel(
        IMediaPlaybackService playbackService,
        IFileDialogService fileDialogService,
        IUrlInputDialogService urlInputDialogService,
        IMessageBoxService messageBoxService,
        ISampleProviderService sampleProviderService)
    {
        _playbackService = playbackService ?? throw new ArgumentNullException(nameof(playbackService));
        _fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
        _urlInputDialogService = urlInputDialogService ?? throw new ArgumentNullException(nameof(urlInputDialogService));
        _messageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
        _sampleProviderService = sampleProviderService ?? throw new ArgumentNullException(nameof(sampleProviderService));

        SampleItems = _sampleProviderService.GetSampleMediaItems();

        foreach (MediaItem sample in SampleItems)
        {
            Playlist.Add(sample);
        }

        _volume = _playbackService.Volume;
        _isMuted = _playbackService.IsMuted;

        _playbackService.StateChanged += OnPlaybackStateChanged;
        _playbackService.PositionChanged += OnPlaybackPositionChanged;
        _playbackService.DurationChanged += OnPlaybackDurationChanged;
        _playbackService.PlaybackErrorOccurred += OnPlaybackErrorOccurred;
        _playbackService.MediaEnded += OnPlaybackMediaEnded;

        OpenFileCommand = new RelayCommand(ExecuteOpenFile);
        OpenUrlCommand = new RelayCommand(ExecuteOpenUrl);
        PlayPauseCommand = new RelayCommand(ExecutePlayPause, () => HasMedia);
        StopCommand = new RelayCommand(ExecuteStop, () => HasMedia);
        PlayNextCommand = new RelayCommand(ExecutePlayNext, () => Playlist.Count > 0);
        PlayPreviousCommand = new RelayCommand(ExecutePlayPrevious, () => Playlist.Count > 0 || HasMedia);
        StepForwardCommand = PlayNextCommand;
        StepBackwardCommand = PlayPreviousCommand;
        JumpForwardCommand = new RelayCommand(ExecuteJumpForward, () => HasMedia);
        JumpBackwardCommand = new RelayCommand(ExecuteJumpBackward, () => HasMedia);
        ToggleMuteCommand = new RelayCommand(ExecuteToggleMute);
        BeginSeekCommand = new RelayCommand(ExecuteBeginSeek);
        EndSeekCommand = new RelayCommand(ExecuteEndSeek);
        CommitDirectSeekCommand = new RelayCommand<double?>(ExecuteCommitDirectSeek);
        LoadSampleCommand = new RelayCommand<MediaItem>(ExecuteLoadSample);
        ToggleFullscreenCommand = new RelayCommand(ExecuteToggleFullscreen);
    }

    public void LoadMedia(Uri uri, string title, string description)
    {
        MediaType detectedType = MediaItem.DetectMediaType(uri.OriginalString);
        var item = new MediaItem(uri, title, detectedType, description);

        int existingIndex = FindPlaylistItemIndex(item.Uri);
        if (existingIndex >= 0)
        {
            CurrentPlaylistIndex = existingIndex;
        }
        else
        {
            Playlist.Add(item);
            CurrentPlaylistIndex = Playlist.Count - 1;
        }

        CurrentMediaItem = item;
        _playbackService.Open(item);
    }

    private void ExecuteOpenFile()
    {
        string? selectedPath = _fileDialogService.OpenMediaFile();
        if (!string.IsNullOrWhiteSpace(selectedPath))
        {
            PopulatePlaylistFromDirectory(selectedPath);

            var uri = new Uri(selectedPath, UriKind.Absolute);
            string fileName = Path.GetFileName(selectedPath);
            LoadMedia(uri, fileName, selectedPath);
        }
    }

    private void PopulatePlaylistFromDirectory(string targetFilePath)
    {
        string? directory = Path.GetDirectoryName(targetFilePath);
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
        {
            return;
        }

        Playlist.Clear();

        string[] files = Directory.GetFiles(directory);
        Array.Sort(files);

        foreach (string file in files)
        {
            MediaType type = MediaItem.DetectMediaType(file);
            if (type != MediaType.Unknown)
            {
                var uri = new Uri(file, UriKind.Absolute);
                string name = Path.GetFileName(file);
                Playlist.Add(new MediaItem(uri, name, type, file));
            }
        }
    }

    private void ExecuteOpenUrl()
    {
        string? url = _urlInputDialogService.PromptForMediaUrl();
        if (!string.IsNullOrWhiteSpace(url))
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
            {
                string title = Path.GetFileName(uri.LocalPath);
                if (string.IsNullOrWhiteSpace(title))
                {
                    title = "Remote Network Stream";
                }
                LoadMedia(uri, title, url);
            }
            else
            {
                _messageBoxService.ShowError("The specified URL is not a valid absolute URI.", "Invalid URL");
            }
        }
    }

    private void ExecuteLoadSample(MediaItem? sample)
    {
        if (sample != null)
        {
            int index = Playlist.IndexOf(sample);
            if (index >= 0)
            {
                CurrentPlaylistIndex = index;
            }
            CurrentMediaItem = sample;
            _playbackService.Open(sample);
        }
    }

    private void ExecutePlayPause()
    {
        if (PlaybackState == PlaybackState.Playing)
        {
            _playbackService.Pause();
        }
        else
        {
            _playbackService.Play();
        }
    }

    private void ExecuteStop()
    {
        _playbackService.Stop();
        ProgressValue = 0;
        CurrentTimeDisplay = "00:00";
    }

    private void ExecutePlayNext()
    {
        if (Playlist.Count == 0)
        {
            return;
        }

        int nextIndex = (CurrentPlaylistIndex + 1) % Playlist.Count;
        CurrentPlaylistIndex = nextIndex;
        MediaItem targetItem = Playlist[nextIndex];
        CurrentMediaItem = targetItem;
        _playbackService.Open(targetItem);
    }

    private void ExecutePlayPrevious()
    {
        if (Playlist.Count == 0)
        {
            return;
        }

        if (CurrentPosition > TimeSpan.FromSeconds(3))
        {
            _playbackService.Seek(TimeSpan.Zero);
            return;
        }

        int prevIndex = CurrentPlaylistIndex <= 0 ? Playlist.Count - 1 : CurrentPlaylistIndex - 1;
        CurrentPlaylistIndex = prevIndex;
        MediaItem targetItem = Playlist[prevIndex];
        CurrentMediaItem = targetItem;
        _playbackService.Open(targetItem);
    }

    private void ExecuteJumpForward()
    {
        _playbackService.StepForward(TimeSpan.FromSeconds(5));
    }

    private void ExecuteJumpBackward()
    {
        _playbackService.StepBackward(TimeSpan.FromSeconds(5));
    }

    private void ExecuteToggleMute()
    {
        IsMuted = !IsMuted;
    }

    private void ExecuteBeginSeek()
    {
        _isUserDraggingProgress = true;
    }

    private void ExecuteEndSeek()
    {
        _isUserDraggingProgress = false;
        CurrentTimeDisplay = FormatTimeSpan(TimeSpan.FromSeconds(ProgressValue));
        _playbackService.Seek(TimeSpan.FromSeconds(ProgressValue));
    }

    private void ExecuteCommitDirectSeek(double? value)
    {
        if (value.HasValue)
        {
            ProgressValue = value.Value;
            CurrentTimeDisplay = FormatTimeSpan(TimeSpan.FromSeconds(value.Value));
            _playbackService.Seek(TimeSpan.FromSeconds(value.Value));
        }
    }

    private void ExecuteToggleFullscreen()
    {
        IsFullscreen = !IsFullscreen;
    }

    private void OnPlaybackStateChanged(object? sender, PlaybackState state)
    {
        PlaybackState = state;
    }

    private void OnPlaybackPositionChanged(object? sender, TimeSpan position)
    {
        CurrentPosition = position;
    }

    private void OnPlaybackDurationChanged(object? sender, TimeSpan duration)
    {
        Duration = duration;
    }

    private void OnPlaybackErrorOccurred(object? sender, string errorMessage)
    {
        _messageBoxService.ShowError(errorMessage, "Playback Error");
    }

    private void OnPlaybackMediaEnded(object? sender, EventArgs e)
    {
        if (Playlist.Count > 1)
        {
            ExecutePlayNext();
        }
    }

    private int FindPlaylistItemIndex(Uri uri)
    {
        for (int i = 0; i < Playlist.Count; i++)
        {
            if (Playlist[i].Uri == uri)
            {
                return i;
            }
        }
        return -1;
    }

    private static string FormatTimeSpan(TimeSpan timeSpan)
    {
        if (timeSpan < TimeSpan.Zero)
        {
            timeSpan = TimeSpan.Zero;
        }

        if (timeSpan.TotalHours >= 1)
        {
            return timeSpan.ToString(@"hh\:mm\:ss");
        }

        return timeSpan.ToString(@"mm\:ss");
    }
}
