using MediaPlayerApp.Models;
using MediaPlayerApp.Tests.Fakes;
using MediaPlayerApp.ViewModels;

namespace MediaPlayerApp.Tests;

public class MainViewModelTests
{
    private readonly FakeMediaPlaybackService _playbackService;
    private readonly FakeFileDialogService _fileDialogService;
    private readonly FakeUrlInputDialogService _urlInputDialogService;
    private readonly FakeMessageBoxService _messageBoxService;
    private readonly FakeSampleProviderService _sampleProviderService;
    private readonly MainViewModel _viewModel;

    public MainViewModelTests()
    {
        _playbackService = new FakeMediaPlaybackService();
        _fileDialogService = new FakeFileDialogService();
        _urlInputDialogService = new FakeUrlInputDialogService();
        _messageBoxService = new FakeMessageBoxService();
        _sampleProviderService = new FakeSampleProviderService();

        _viewModel = new MainViewModel(
            _playbackService,
            _fileDialogService,
            _urlInputDialogService,
            _messageBoxService,
            _sampleProviderService
        );
    }

    [Fact]
    public void InitialState_HasNoMediaAndStopped()
    {
        Assert.False(_viewModel.HasMedia);
        Assert.False(_viewModel.IsPlaying);
        Assert.Equal(PlaybackState.Stopped, _viewModel.PlaybackState);
        Assert.Equal("00:00", _viewModel.CurrentTimeDisplay);
        Assert.Equal("00:00", _viewModel.TotalTimeDisplay);
    }

    [Fact]
    public void LoadMedia_UpdatesCurrentMediaItemAndStartsPlayback()
    {
        var uri = new Uri("C:/media/test.mp4");
        _viewModel.LoadMedia(uri, "test.mp4", "local test file");

        Assert.True(_viewModel.HasMedia);
        Assert.True(_viewModel.IsVideoVisible);
        Assert.False(_viewModel.IsAudioPlaceholderVisible);
        Assert.Equal("test.mp4", _viewModel.MediaTitleDisplay);
        Assert.Equal(PlaybackState.Playing, _viewModel.PlaybackState);
    }

    [Fact]
    public void LoadMedia_AudioFile_SetsAudioPlaceholderVisible()
    {
        var uri = new Uri("C:/media/test.mp3");
        _viewModel.LoadMedia(uri, "test.mp3", "local audio file");

        Assert.True(_viewModel.HasMedia);
        Assert.False(_viewModel.IsVideoVisible);
        Assert.True(_viewModel.IsAudioPlaceholderVisible);
    }

    [Fact]
    public void PlayPauseCommand_TogglesPlaybackState()
    {
        var uri = new Uri("C:/media/test.mp4");
        _viewModel.LoadMedia(uri, "test.mp4", "local test file");
        Assert.Equal(PlaybackState.Playing, _viewModel.PlaybackState);

        _viewModel.PlayPauseCommand.Execute(null);
        Assert.True(_playbackService.PauseCalled);
        Assert.Equal(PlaybackState.Paused, _viewModel.PlaybackState);

        _viewModel.PlayPauseCommand.Execute(null);
        Assert.True(_playbackService.PlayCalled);
        Assert.Equal(PlaybackState.Playing, _viewModel.PlaybackState);
    }

    [Fact]
    public void StopCommand_StopsPlaybackAndResetsProgress()
    {
        var uri = new Uri("C:/media/test.mp4");
        _viewModel.LoadMedia(uri, "test.mp4", "local test file");
        _viewModel.ProgressValue = 42.0;

        _viewModel.StopCommand.Execute(null);

        Assert.True(_playbackService.StopCalled);
        Assert.Equal(PlaybackState.Stopped, _viewModel.PlaybackState);
        Assert.Equal(0, _viewModel.ProgressValue);
        Assert.Equal("00:00", _viewModel.CurrentTimeDisplay);
    }

    [Fact]
    public void JumpForwardCommand_AdvancesByFiveSeconds()
    {
        var uri = new Uri("C:/media/test.mp4");
        _viewModel.LoadMedia(uri, "test.mp4", "local test");
        _playbackService.CurrentPosition = TimeSpan.FromSeconds(30);

        _viewModel.JumpForwardCommand.Execute(null);

        Assert.Equal(TimeSpan.FromSeconds(5), _playbackService.LastSteppedForward);
        Assert.Equal(TimeSpan.FromSeconds(35), _playbackService.LastSeekedPosition);
    }

    [Fact]
    public void JumpBackwardCommand_RewindsByFiveSeconds()
    {
        var uri = new Uri("C:/media/test.mp4");
        _viewModel.LoadMedia(uri, "test.mp4", "local test");
        _playbackService.CurrentPosition = TimeSpan.FromSeconds(30);

        _viewModel.JumpBackwardCommand.Execute(null);

        Assert.Equal(TimeSpan.FromSeconds(5), _playbackService.LastSteppedBackward);
        Assert.Equal(TimeSpan.FromSeconds(25), _playbackService.LastSeekedPosition);
    }

    [Fact]
    public void PlayNextCommand_SwitchesToNextMediaInPlaylist()
    {
        var item1 = new MediaItem(new Uri("C:/media/video1.mp4"), "Video 1", MediaType.Video, "Test");
        var item2 = new MediaItem(new Uri("C:/media/video2.mp4"), "Video 2", MediaType.Video, "Test");

        _viewModel.Playlist.Clear();
        _viewModel.Playlist.Add(item1);
        _viewModel.Playlist.Add(item2);

        _viewModel.LoadMedia(item1.Uri, item1.Title, item1.SourceDescription);
        Assert.Equal("Video 1", _viewModel.MediaTitleDisplay);

        _viewModel.PlayNextCommand.Execute(null);

        Assert.Equal("Video 2", _viewModel.MediaTitleDisplay);
        Assert.Equal(1, _viewModel.CurrentPlaylistIndex);
    }

    [Fact]
    public void PlayPreviousCommand_SwitchesToPreviousMediaInPlaylist()
    {
        var item1 = new MediaItem(new Uri("C:/media/video1.mp4"), "Video 1", MediaType.Video, "Test");
        var item2 = new MediaItem(new Uri("C:/media/video2.mp4"), "Video 2", MediaType.Video, "Test");

        _viewModel.Playlist.Clear();
        _viewModel.Playlist.Add(item1);
        _viewModel.Playlist.Add(item2);

        _viewModel.LoadMedia(item2.Uri, item2.Title, item2.SourceDescription);
        _playbackService.CurrentPosition = TimeSpan.FromSeconds(1);

        _viewModel.PlayPreviousCommand.Execute(null);

        Assert.Equal("Video 1", _viewModel.MediaTitleDisplay);
        Assert.Equal(0, _viewModel.CurrentPlaylistIndex);
    }

    [Fact]
    public void PlayPreviousCommand_RestartsCurrentMedia_WhenPositionGreaterThanThreeSeconds()
    {
        var item = new MediaItem(new Uri("C:/media/video1.mp4"), "Video 1", MediaType.Video, "Test");
        _viewModel.LoadMedia(item.Uri, item.Title, item.SourceDescription);
        _playbackService.CurrentPosition = TimeSpan.FromSeconds(15);

        _viewModel.PlayPreviousCommand.Execute(null);

        Assert.Equal(TimeSpan.Zero, _playbackService.LastSeekedPosition);
        Assert.Equal("Video 1", _viewModel.MediaTitleDisplay);
    }

    [Fact]
    public void Volume_UpdatesPlaybackServiceAndClampsBounds()
    {
        _viewModel.Volume = 0.5;
        Assert.Equal(0.5, _playbackService.Volume);

        _viewModel.Volume = 1.5;
        Assert.Equal(1.0, _viewModel.Volume);
        Assert.Equal(1.0, _playbackService.Volume);

        _viewModel.Volume = -0.5;
        Assert.Equal(0.0, _viewModel.Volume);
        Assert.Equal(0.0, _playbackService.Volume);
    }

    [Fact]
    public void ToggleMuteCommand_TogglesMuteState()
    {
        Assert.False(_viewModel.IsMuted);

        _viewModel.ToggleMuteCommand.Execute(null);
        Assert.True(_viewModel.IsMuted);
        Assert.True(_playbackService.IsMuted);

        _viewModel.ToggleMuteCommand.Execute(null);
        Assert.False(_viewModel.IsMuted);
        Assert.False(_playbackService.IsMuted);
    }

    [Fact]
    public void ProgressScrubbing_SuspendsPositionOverwritesDuringDrag()
    {
        var uri = new Uri("C:/media/test.mp4");
        _viewModel.LoadMedia(uri, "test.mp4", "local test");

        _viewModel.BeginSeekCommand.Execute(null);
        _viewModel.ProgressValue = 75.0;

        _playbackService.CurrentPosition = TimeSpan.FromSeconds(10);

        Assert.Equal(75.0, _viewModel.ProgressValue);
        Assert.Equal("01:15", _viewModel.CurrentTimeDisplay);

        _viewModel.EndSeekCommand.Execute(null);

        Assert.Equal(TimeSpan.FromSeconds(75), _playbackService.LastSeekedPosition);
    }

    [Fact]
    public void CommitDirectSeekCommand_DirectlySeeksPosition()
    {
        var uri = new Uri("C:/media/test.mp4");
        _viewModel.LoadMedia(uri, "test.mp4", "local test");

        _viewModel.CommitDirectSeekCommand.Execute(55.0);

        Assert.Equal(55.0, _viewModel.ProgressValue);
        Assert.Equal(TimeSpan.FromSeconds(55), _playbackService.LastSeekedPosition);
    }

    [Fact]
    public void PlaybackError_TriggersMessageBoxService()
    {
        _playbackService.RaiseError("Codec decode failed.");

        Assert.Single(_messageBoxService.Errors);
        Assert.Equal("Codec decode failed.", _messageBoxService.Errors[0].Message);
        Assert.Equal("Playback Error", _messageBoxService.Errors[0].Title);
    }

    [Fact]
    public void OpenFileCommand_LoadsSelectedFile()
    {
        _fileDialogService.FileToReturn = "C:/Videos/sample.mp4";

        _viewModel.OpenFileCommand.Execute(null);

        Assert.True(_viewModel.HasMedia);
        Assert.Equal("sample.mp4", _viewModel.MediaTitleDisplay);
        Assert.Equal(MediaType.Video, _viewModel.CurrentMediaItem?.MediaType);
    }

    [Fact]
    public void OpenUrlCommand_LoadsSelectedUrl()
    {
        _urlInputDialogService.UrlToReturn = "https://example.com/audio.mp3";

        _viewModel.OpenUrlCommand.Execute(null);

        Assert.True(_viewModel.HasMedia);
        Assert.Equal("audio.mp3", _viewModel.MediaTitleDisplay);
        Assert.Equal(MediaType.Audio, _viewModel.CurrentMediaItem?.MediaType);
    }
}
