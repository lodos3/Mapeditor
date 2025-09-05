using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mir2.Core.Services;
using Mir2.Core.IO;
using System;
using System.IO;
using System.Threading.Tasks;
using Mir2.Editor.Views;
using Mir2.Editor.ViewModels;
using Mir2.Editor.Services;

namespace Mir2.Editor;

class Program
{
    private static IHost? _host;

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // Initialize logging
        try
        {
            StartupLogger.LogStartup("=== Map Editor Startup Log ===");
            StartupLogger.LogStartup($"Application starting at: {DateTime.Now}");
            StartupLogger.LogStartup($"Command line args: {string.Join(" ", args)}");
            StartupLogger.LogStartup($"Current directory: {Environment.CurrentDirectory}");
            StartupLogger.LogStartup($"Runtime version: {Environment.Version}");
            StartupLogger.LogStartup($"Platform: {Environment.OSVersion}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize logging: {ex.Message}");
        }

        try
        {
            StartupLogger.LogStartup("Building Avalonia application...");
            var app = BuildAvaloniaApp();
            StartupLogger.LogStartup("Avalonia app built successfully");

            StartupLogger.LogStartup("Starting with classic desktop lifetime...");
            app.StartWithClassicDesktopLifetime(args);
            StartupLogger.LogStartup("Application exited normally");
        }
        catch (Exception ex)
        {
            var errorMsg = $"FATAL ERROR during application startup: {ex}";
            StartupLogger.LogStartup(errorMsg);
            Console.WriteLine(errorMsg);
            
            // Check if this is a display-related error
            if (ex.Message.Contains("XOpenDisplay") || ex.Message.Contains("Display"))
            {
                Console.WriteLine("\n=== DISPLAY ERROR DETECTED ===");
                Console.WriteLine("The application requires a display server to run.");
                Console.WriteLine("This is likely because:");
                Console.WriteLine("1. Running in a headless environment (no GUI)");
                Console.WriteLine("2. No X11 display server is available");
                Console.WriteLine("3. DISPLAY environment variable is not set");
                Console.WriteLine("\nSolutions:");
                Console.WriteLine("1. Use VNC or remote desktop to provide a display");
                Console.WriteLine("2. Set up X11 forwarding if using SSH");
                Console.WriteLine("3. Use a virtual display (Xvfb) for automated testing");
                Console.WriteLine($"\nDetailed logs have been saved to: {StartupLogger.StartupLogFile}");
                Console.WriteLine("=== END DISPLAY ERROR INFO ===\n");
            }
            
            Console.WriteLine("\nStartup failed. Press any key to exit...");
            try
            {
                Console.ReadKey();
            }
            catch (InvalidOperationException)
            {
                // Handle case when running in non-interactive environment
                Console.WriteLine("Running in non-interactive mode, exiting...");
            }
            Environment.Exit(1);
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        try
        {
            StartupLogger.LogStartup("Configuring Avalonia app builder...");
            var builder = AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
                .UseReactiveUI();
            
            StartupLogger.LogStartup("Avalonia app builder configured successfully");
            return builder;
        }
        catch (Exception ex)
        {
            StartupLogger.LogStartup($"ERROR in BuildAvaloniaApp: {ex}");
            throw;
        }
    }

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
                    services.AddSingleton<EditorService>();
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
