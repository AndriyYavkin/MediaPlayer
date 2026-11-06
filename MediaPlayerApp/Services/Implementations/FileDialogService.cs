using Microsoft.Win32;
using MediaPlayerApp.Services.Interfaces;

namespace MediaPlayerApp.Services.Implementations;

/// <summary>
/// Implements file selection using standard Windows open file dialog.
/// </summary>
public sealed class FileDialogService : IFileDialogService
{
    private const string FilterString =
        "All Supported Media|*.mp3;*.wav;*.flac;*.aac;*.wma;*.ogg;*.mp4;*.avi;*.mkv;*.wmv;*.mov;*.webm|" +
        "Audio Files (*.mp3;*.wav;*.flac;*.aac;*.wma;*.ogg)|*.mp3;*.wav;*.flac;*.aac;*.wma;*.ogg|" +
        "Video Files (*.mp4;*.avi;*.mkv;*.wmv;*.mov;*.webm)|*.mp4;*.avi;*.mkv;*.wmv;*.mov;*.webm|" +
        "All Files (*.*)|*.*";

    public string? OpenMediaFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Open Media File",
            Filter = FilterString,
            Multiselect = false,
            CheckFileExists = true,
            CheckPathExists = true
        };

        bool? result = dialog.ShowDialog();
        return result == true ? dialog.FileName : null;
    }
}
