using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MediaPlayerApp.Services.Interfaces;
using MediaPlayerApp.ViewModels;

namespace MediaPlayerApp;

/// <summary>
/// Interaction logic for MainWindow.xaml.
/// </summary>
public partial class MainWindow : Window
{
    private readonly IMediaPlayerHost _mediaPlayerHost;
    private WindowState _previousWindowState = WindowState.Normal;
    private WindowStyle _previousWindowStyle = WindowStyle.SingleBorderWindow;
    private ResizeMode _previousResizeMode = ResizeMode.CanResize;

    public MainViewModel ViewModel => (MainViewModel)DataContext;

    public MainWindow(MainViewModel viewModel, IMediaPlayerHost mediaPlayerHost)
    {
        InitializeComponent();

        _mediaPlayerHost = mediaPlayerHost ?? throw new ArgumentNullException(nameof(mediaPlayerHost));
        DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

        _mediaPlayerHost.AttachMediaElement(MediaDisplayElement);

        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.IsFullscreen))
        {
            ApplyFullscreenState(ViewModel.IsFullscreen);
        }
    }

    private void ApplyFullscreenState(bool isFullscreen)
    {
        if (isFullscreen)
        {
            _previousWindowState = WindowState;
            _previousWindowStyle = WindowStyle;
            _previousResizeMode = ResizeMode;

            HeaderRow.Height = new GridLength(0);
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            WindowState = WindowState.Maximized;
        }
        else
        {
            HeaderRow.Height = GridLength.Auto;
            WindowStyle = _previousWindowStyle;
            ResizeMode = _previousResizeMode;
            WindowState = _previousWindowState;
        }
    }

    private void OnTimelineDragStarted(object sender, DragStartedEventArgs e)
    {
        ViewModel.BeginSeekCommand.Execute(null);
    }

    private void OnTimelineDragCompleted(object sender, DragCompletedEventArgs e)
    {
        ViewModel.EndSeekCommand.Execute(null);
    }

    private void OnTimelinePreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        ViewModel.CommitDirectSeekCommand.Execute(TimelineSlider.Value);
    }

    private void OnMediaDisplayMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ViewModel.ToggleFullscreenCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void OnWindowDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[]? files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0)
            {
                string filePath = files[0];
                var uri = new Uri(filePath, UriKind.Absolute);
                string fileName = Path.GetFileName(filePath);
                ViewModel.LoadMedia(uri, fileName, filePath);
            }
        }
    }

    private void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            if (e.Key == Key.O)
            {
                ViewModel.OpenFileCommand.Execute(null);
                e.Handled = true;
                return;
            }
            if (e.Key == Key.U)
            {
                ViewModel.OpenUrlCommand.Execute(null);
                e.Handled = true;
                return;
            }
            if (e.Key == Key.Left)
            {
                if (ViewModel.PlayPreviousCommand.CanExecute(null))
                {
                    ViewModel.PlayPreviousCommand.Execute(null);
                    e.Handled = true;
                    return;
                }
            }
            if (e.Key == Key.Right)
            {
                if (ViewModel.PlayNextCommand.CanExecute(null))
                {
                    ViewModel.PlayNextCommand.Execute(null);
                    e.Handled = true;
                    return;
                }
            }
        }

        switch (e.Key)
        {
            case Key.Space:
                if (ViewModel.PlayPauseCommand.CanExecute(null))
                {
                    ViewModel.PlayPauseCommand.Execute(null);
                    e.Handled = true;
                }
                break;

            case Key.Left:
                if (ViewModel.JumpBackwardCommand.CanExecute(null))
                {
                    ViewModel.JumpBackwardCommand.Execute(null);
                    e.Handled = true;
                }
                break;

            case Key.Right:
                if (ViewModel.JumpForwardCommand.CanExecute(null))
                {
                    ViewModel.JumpForwardCommand.Execute(null);
                    e.Handled = true;
                }
                break;

            case Key.PageUp or Key.MediaPreviousTrack:
                if (ViewModel.PlayPreviousCommand.CanExecute(null))
                {
                    ViewModel.PlayPreviousCommand.Execute(null);
                    e.Handled = true;
                }
                break;

            case Key.PageDown or Key.MediaNextTrack:
                if (ViewModel.PlayNextCommand.CanExecute(null))
                {
                    ViewModel.PlayNextCommand.Execute(null);
                    e.Handled = true;
                }
                break;

            case Key.Up:
                ViewModel.Volume += 0.05;
                e.Handled = true;
                break;

            case Key.Down:
                ViewModel.Volume -= 0.05;
                e.Handled = true;
                break;

            case Key.M:
                ViewModel.ToggleMuteCommand.Execute(null);
                e.Handled = true;
                break;

            case Key.F or Key.F11:
                ViewModel.ToggleFullscreenCommand.Execute(null);
                e.Handled = true;
                break;

            case Key.Escape:
                if (ViewModel.IsFullscreen)
                {
                    ViewModel.IsFullscreen = false;
                    e.Handled = true;
                }
                break;
        }
    }
}
