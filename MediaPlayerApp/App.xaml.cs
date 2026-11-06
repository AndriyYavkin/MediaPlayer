using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using MediaPlayerApp.Services.Implementations;
using MediaPlayerApp.Services.Interfaces;
using MediaPlayerApp.ViewModels;

namespace MediaPlayerApp;

/// <summary>
/// Interaction logic for App.xaml and IoC container composition root.
/// </summary>
public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        AppContext.SetSwitch("Switch.System.Windows.Net.DoNotApplyZoneCheckForDefaultCredentials", true);

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IRemoteMediaStreamService, RemoteMediaStreamService>();
        services.AddSingleton<WpfMediaPlaybackService>();
        services.AddSingleton<IMediaPlaybackService>(sp => sp.GetRequiredService<WpfMediaPlaybackService>());
        services.AddSingleton<IMediaPlayerHost>(sp => sp.GetRequiredService<WpfMediaPlaybackService>());

        services.AddSingleton<IFileDialogService, FileDialogService>();
        services.AddSingleton<IUrlInputDialogService, UrlInputDialogService>();
        services.AddSingleton<IMessageBoxService, MessageBoxService>();
        services.AddSingleton<ISampleProviderService, SampleProviderService>();

        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindow>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}
