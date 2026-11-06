using System.Collections.ObjectModel;
using System.Windows.Input;
using MediaPlayerApp.Common;

namespace MediaPlayerApp.ViewModels;

/// <summary>
/// ViewModel managing remote URL entry and preset selection.
/// </summary>
public sealed class UrlInputDialogViewModel : ObservableObject
{
    private string _urlText = string.Empty;
    private string? _errorMessage;

    public string UrlText
    {
        get => _urlText;
        set
        {
            if (SetProperty(ref _urlText, value))
            {
                ErrorMessage = null;
            }
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ObservableCollection<UrlPreset> Presets { get; } = new();

    public ICommand ConfirmCommand { get; }
    public ICommand SelectPresetCommand { get; }

    public event EventHandler<bool>? RequestClose;

    public UrlInputDialogViewModel(string? initialUrl = null)
    {
        _urlText = initialUrl ?? "https://raw.githubusercontent.com/mediaelement/mediaelement-files/master/big_buck_bunny.mp4";

        Presets.Add(new UrlPreset(
            "Big Buck Bunny (MP4 Video)",
            "https://raw.githubusercontent.com/mediaelement/mediaelement-files/master/big_buck_bunny.mp4"
        ));
        Presets.Add(new UrlPreset(
            "Blooming Flower (MP4 Video)",
            "https://interactive-examples.mdn.mozilla.net/media/cc0-videos/flower.mp4"
        ));
        Presets.Add(new UrlPreset(
            "Friday Animation (MP4 Video)",
            "https://interactive-examples.mdn.mozilla.net/media/cc0-videos/friday.mp4"
        ));
        Presets.Add(new UrlPreset(
            "Viper Soundtrack (MP3 Audio)",
            "https://raw.githubusercontent.com/mdn/learning-area/master/html/multimedia-and-embedding/video-and-audio-content/viper.mp3"
        ));

        ConfirmCommand = new RelayCommand(ExecuteConfirm);
        SelectPresetCommand = new RelayCommand<UrlPreset>(ExecuteSelectPreset);
    }

    private void ExecuteSelectPreset(UrlPreset? preset)
    {
        if (preset != null)
        {
            UrlText = preset.Url;
        }
    }

    private void ExecuteConfirm()
    {
        string trimmed = UrlText.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            ErrorMessage = "URL cannot be empty.";
            return;
        }

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out Uri? parsedUri) ||
            (parsedUri.Scheme != Uri.UriSchemeHttp && parsedUri.Scheme != Uri.UriSchemeHttps))
        {
            ErrorMessage = "Please enter a valid HTTP or HTTPS URL.";
            return;
        }

        RequestClose?.Invoke(this, true);
    }
}

/// <summary>
/// Encapsulates a quick preset URL for convenient playback testing.
/// </summary>
public sealed record UrlPreset(string Name, string Url);
