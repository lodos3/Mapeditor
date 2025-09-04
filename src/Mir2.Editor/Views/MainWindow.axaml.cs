using Avalonia.Controls;
using Avalonia.Interactivity;
using Mir2.Editor.ViewModels;
using System;
using System.IO;
using System.Reactive.Linq;

namespace Mir2.Editor.Views;

public partial class MainWindow : Window
{
    private static readonly string LogsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mir2Editor", "Logs");
    private static readonly string StartupLogFile = Path.Combine(LogsDirectory, $"startup_{DateTime.Now:yyyyMMdd_HHmmss}.log");

    public MainWindow()
    {
        try
        {
            LogStartup("MainWindow constructor called - Initializing components...");
            InitializeComponent();
            LogStartup("MainWindow.InitializeComponent() completed successfully");
            LogStartup($"MainWindow created successfully - Title: {Title ?? "null"}");
        }
        catch (Exception ex)
        {
            LogStartup($"ERROR in MainWindow constructor: {ex}");
            throw;
        }
    }

    private void OnInitializeClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.InitializeCommand.Execute().Subscribe();
        }
    }

    private void OnLoadMapClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.LoadMapCommand.Execute().Subscribe();
        }
    }

    private void OnSaveMapClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.SaveMapCommand.Execute().Subscribe();
        }
    }

    private void OnNewMapClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.NewMapCommand.Execute().Subscribe();
        }
    }

    private void OnUndoClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.UndoCommand.Execute().Subscribe();
        }
    }

    private void OnRedoClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.RedoCommand.Execute().Subscribe();
        }
    }

    private static void LogStartup(string message)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var logMessage = $"[{timestamp}] [MAINWINDOW] {message}";
        
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