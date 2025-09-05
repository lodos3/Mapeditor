using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Mir2.Editor.ViewModels;
using Mir2.Editor.Views;
using Mir2.Editor.Services;
using System;

namespace Mir2.Editor;

public partial class App : Application
{
    public override void Initialize()
    {
        try
        {
            StartupLogger.LogStartup("App.Initialize() called - Loading XAML...", "APP");
            
            // Initialize the application logger for runtime logging
            ApplicationLogger.Initialize();
            
            AvaloniaXamlLoader.Load(this);
            StartupLogger.LogStartup("App.Initialize() completed successfully", "APP");
            ApplicationLogger.LogInfo("Application XAML loaded successfully", "APP");
        }
        catch (Exception ex)
        {
            StartupLogger.LogStartup($"ERROR in App.Initialize(): {ex}", "APP");
            ApplicationLogger.LogError("Failed to initialize application", ex, "APP");
            throw;
        }
    }

    public override void OnFrameworkInitializationCompleted()
    {
        try
        {
            StartupLogger.LogStartup("App.OnFrameworkInitializationCompleted() called", "APP");
            ApplicationLogger.LogInfo("Framework initialization starting", "APP");
            
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                StartupLogger.LogStartup("Creating MainWindow...", "APP");
                ApplicationLogger.LogInfo("Creating main window", "APP");
                
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
                
                StartupLogger.LogStartup("MainWindow created successfully", "APP");
                ApplicationLogger.LogInfo("Main window created successfully", "APP");
            }
            else
            {
                var warningMsg = $"ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime, type: {ApplicationLifetime?.GetType().Name ?? "null"}";
                StartupLogger.LogStartup($"WARNING: {warningMsg}", "APP");
                ApplicationLogger.LogWarning(warningMsg, "APP");
            }

            StartupLogger.LogStartup("Calling base.OnFrameworkInitializationCompleted()...", "APP");
            base.OnFrameworkInitializationCompleted();
            
            StartupLogger.LogStartup("App.OnFrameworkInitializationCompleted() completed successfully", "APP");
            ApplicationLogger.LogInfo("Framework initialization completed successfully", "APP");
        }
        catch (Exception ex)
        {
            StartupLogger.LogStartup($"ERROR in App.OnFrameworkInitializationCompleted(): {ex}", "APP");
            ApplicationLogger.LogError("Framework initialization failed", ex, "APP");
            throw;
        }
    }
}