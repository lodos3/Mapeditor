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
            AvaloniaXamlLoader.Load(this);
            StartupLogger.LogStartup("App.Initialize() completed successfully", "APP");
        }
        catch (Exception ex)
        {
            StartupLogger.LogStartup($"ERROR in App.Initialize(): {ex}", "APP");
            throw;
        }
    }

    public override void OnFrameworkInitializationCompleted()
    {
        try
        {
            StartupLogger.LogStartup("App.OnFrameworkInitializationCompleted() called", "APP");
            
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                StartupLogger.LogStartup("Creating MainWindow...", "APP");
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
                StartupLogger.LogStartup("MainWindow created successfully", "APP");
            }
            else
            {
                StartupLogger.LogStartup($"WARNING: ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime, type: {ApplicationLifetime?.GetType().Name ?? "null"}", "APP");
            }

            StartupLogger.LogStartup("Calling base.OnFrameworkInitializationCompleted()...", "APP");
            base.OnFrameworkInitializationCompleted();
            StartupLogger.LogStartup("App.OnFrameworkInitializationCompleted() completed successfully", "APP");
        }
        catch (Exception ex)
        {
            StartupLogger.LogStartup($"ERROR in App.OnFrameworkInitializationCompleted(): {ex}", "APP");
            throw;
        }
    }
}