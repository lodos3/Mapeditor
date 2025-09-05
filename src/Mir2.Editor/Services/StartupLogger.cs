using System;
using System.IO;

namespace Mir2.Editor.Services;

public static class StartupLogger
{
    private static readonly string LogsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mir2Editor", "Logs");
    private static readonly Lazy<string> _startupLogFile = new(() => Path.Combine(LogsDirectory, $"startup_{DateTime.Now:yyyyMMdd_HHmmss}.log"));
    
    public static string StartupLogFile => _startupLogFile.Value;

    public static void LogStartup(string message, string component = "")
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var componentTag = string.IsNullOrEmpty(component) ? "" : $" [{component}]";
        var logMessage = $"[{timestamp}]{componentTag} {message}";
        
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