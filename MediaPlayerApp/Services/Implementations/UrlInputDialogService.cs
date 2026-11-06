using System.Windows;
using MediaPlayerApp.Dialogs;
using MediaPlayerApp.Services.Interfaces;
using MediaPlayerApp.ViewModels;

namespace MediaPlayerApp.Services.Implementations;

/// <summary>
/// Implements URL input collection by presenting a dedicated modal dialog window.
/// </summary>
public sealed class UrlInputDialogService : IUrlInputDialogService
{
    public string? PromptForMediaUrl(string? defaultUrl = null)
    {
        var viewModel = new UrlInputDialogViewModel(defaultUrl);
        var dialog = new UrlInputDialog(viewModel);

        if (Application.Current?.MainWindow != null)
        {
            dialog.Owner = Application.Current.MainWindow;
        }

        bool? result = dialog.ShowDialog();
        return result == true ? viewModel.UrlText.Trim() : null;
    }
}
