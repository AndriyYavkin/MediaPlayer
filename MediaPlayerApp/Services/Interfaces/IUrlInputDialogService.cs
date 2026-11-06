namespace MediaPlayerApp.Services.Interfaces;

/// <summary>
/// Defines the contract for prompting the user to enter a remote media URL.
/// </summary>
public interface IUrlInputDialogService
{
    string? PromptForMediaUrl(string? defaultUrl = null);
}
