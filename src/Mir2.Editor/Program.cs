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
    private static readonly string LogsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mir2Editor", "Logs");
    private static readonly string StartupLogFile = Path.Combine(LogsDirectory, $"startup_{DateTime.Now:yyyyMMdd_HHmmss}.log");

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // Ensure logs directory exists
        try
        {
            Directory.CreateDirectory(LogsDirectory);
            LogStartup("=== Map Editor Startup Log ===");
            LogStartup($"Application starting at: {DateTime.Now}");
            LogStartup($"Command line args: {string.Join(" ", args)}");
            LogStartup($"Current directory: {Environment.CurrentDirectory}");
            LogStartup($"Runtime version: {Environment.Version}");
            LogStartup($"Platform: {Environment.OSVersion}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize logging: {ex.Message}");
        }

        try
        {
            LogStartup("Building Avalonia application...");
            var app = BuildAvaloniaApp();
            LogStartup("Avalonia app built successfully");

            LogStartup("Starting with classic desktop lifetime...");
            app.StartWithClassicDesktopLifetime(args);
            LogStartup("Application exited normally");
        }
        catch (Exception ex)
        {
            var errorMsg = $"FATAL ERROR during application startup: {ex}";
            LogStartup(errorMsg);
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
                Console.WriteLine($"\nDetailed logs have been saved to: {StartupLogFile}");
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
            LogStartup("Configuring Avalonia app builder...");
            var builder = AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
                .UseReactiveUI();
            
            LogStartup("Avalonia app builder configured successfully");
            return builder;
        }
        catch (Exception ex)
        {
            LogStartup($"ERROR in BuildAvaloniaApp: {ex}");
            throw;
        }
    }

    private static void LogStartup(string message)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var logMessage = $"[{timestamp}] {message}";
        
        // Log to console
        Console.WriteLine(logMessage);
        
        // Log to file
        try
        {
            File.AppendAllText(StartupLogFile, logMessage + Environment.NewLine);
        }
        catch
        {
            // Ignore file logging errors to prevent infinite loops
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
