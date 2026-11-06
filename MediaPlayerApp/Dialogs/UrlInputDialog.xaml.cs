using System.Windows;
using MediaPlayerApp.ViewModels;

namespace MediaPlayerApp.Dialogs;

/// <summary>
/// Interaction logic for UrlInputDialog.xaml.
/// </summary>
public partial class UrlInputDialog : Window
{
    public UrlInputDialog(UrlInputDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.RequestClose += (sender, result) =>
        {
            DialogResult = result;
            Close();
        };
    }
}
