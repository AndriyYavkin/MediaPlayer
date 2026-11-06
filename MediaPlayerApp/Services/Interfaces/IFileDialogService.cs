namespace MediaPlayerApp.Services.Interfaces;

/// <summary>
/// Defines the contract for opening file selection dialogs.
/// </summary>
public interface IFileDialogService
{
    string? OpenMediaFile();
}
