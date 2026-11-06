namespace MediaPlayerApp.Services.Interfaces;

/// <summary>
/// Defines the contract for presenting messages to the user.
/// </summary>
public interface IMessageBoxService
{
    void ShowError(string message, string title);
    void ShowInfo(string message, string title);
}
