using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Mir2.Editor.ViewModels;
using Mir2.Editor.Views;
using System;
using System.IO;

namespace Mir2.Editor;

public partial class App : Application
{
    private static readonly string LogsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mir2Editor", "Logs");
    private static readonly string StartupLogFile = Path.Combine(LogsDirectory, $"startup_{DateTime.Now:yyyyMMdd_HHmmss}.log");

    public override void Initialize()
    {
        try
        {
            LogStartup("App.Initialize() called - Loading XAML...");
            AvaloniaXamlLoader.Load(this);
            LogStartup("App.Initialize() completed successfully");
        }
        catch (Exception ex)
        {
            LogStartup($"ERROR in App.Initialize(): {ex}");
            throw;
        }
    }

    public override void OnFrameworkInitializationCompleted()
    {
        try
        {
            LogStartup("App.OnFrameworkInitializationCompleted() called");
            
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                LogStartup("Creating MainWindow...");
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
                LogStartup("MainWindow created successfully");
            }
            else
            {
                LogStartup($"WARNING: ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime, type: {ApplicationLifetime?.GetType().Name ?? "null"}");
            }

            LogStartup("Calling base.OnFrameworkInitializationCompleted()...");
            base.OnFrameworkInitializationCompleted();
            LogStartup("App.OnFrameworkInitializationCompleted() completed successfully");
        }
        catch (Exception ex)
        {
            LogStartup($"ERROR in App.OnFrameworkInitializationCompleted(): {ex}");
            throw;
        }
    }

    private static void LogStartup(string message)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var logMessage = $"[{timestamp}] [APP] {message}";
        
        // Log to console
        Console.WriteLine(logMessage);
        
        // Log to file
        try
        {
            Directory.CreateDirectory(LogsDirectory);
            File.AppendAllText(StartupLogFile, logMessage + Environment.NewLine);
        }
        catch
        {
            // Ignore file logging errors to prevent infinite loops
        }
    }
}