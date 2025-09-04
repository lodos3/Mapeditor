using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mir2.Core.Services;
using Mir2.Core.IO;
using System;
using System.Threading.Tasks;
using Mir2.Editor.Views;
using Mir2.Editor.ViewModels;

namespace Mir2.Editor;

class Program
{
    private static IHost? _host;

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();

    public static async Task<IHost> GetHostAsync()
    {
        if (_host == null)
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<LibraryCatalog>();
                    services.AddTransient<MapReader>();
                    services.AddTransient<MapWriter>();
                    services.AddTransient<MainWindowViewModel>();
                })
                .Build();

            await _host.StartAsync();
        }

        return _host;
    }

    public static async Task DisposeHostAsync()
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
            _host = null;
        }
    }
}
