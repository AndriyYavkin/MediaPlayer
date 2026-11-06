using System.Windows;
using MediaPlayerApp.Services.Interfaces;

namespace MediaPlayerApp.Services.Implementations;

/// <summary>
/// Implements user messaging using WPF standard message boxes.
/// </summary>
public sealed class MessageBoxService : IMessageBoxService
{
    public void ShowError(string message, string title)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public void ShowInfo(string message, string title)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
