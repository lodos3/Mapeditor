using System;
using System.IO;

namespace Mir2.Editor.Services;

/// <summary>
/// Comprehensive application logger for runtime logging of errors, warnings, messages, usage, and performance
/// </summary>
public static class ApplicationLogger
{
    private static readonly string LogsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mir2Editor", "Logs");
    private static readonly Lazy<string> _runtimeLogFile = new(() => Path.Combine(LogsDirectory, $"runtime_{DateTime.Now:yyyyMMdd_HHmmss}.log"));
    
    public static string RuntimeLogFile => _runtimeLogFile.Value;

    /// <summary>
    /// Log an error message
    /// </summary>
    public static void LogError(string message, Exception? exception = null, string component = "")
    {
        var logMessage = FormatMessage("ERROR", message, component, exception);
        WriteToLog(logMessage);
        
        // Also log to console for immediate visibility
        Console.WriteLine(logMessage);
    }

    /// <summary>
    /// Log a warning message
    /// </summary>
    public static void LogWarning(string message, string component = "")
    {
        var logMessage = FormatMessage("WARNING", message, component);
        WriteToLog(logMessage);
        Console.WriteLine(logMessage);
    }

    /// <summary>
    /// Log an informational message
    /// </summary>
    public static void LogInfo(string message, string component = "")
    {
        var logMessage = FormatMessage("INFO", message, component);
        WriteToLog(logMessage);
        Console.WriteLine(logMessage);
    }

    /// <summary>
    /// Log a usage/activity message
    /// </summary>
    public static void LogUsage(string message, string component = "")
    {
        var logMessage = FormatMessage("USAGE", message, component);
        WriteToLog(logMessage);
    }

    /// <summary>
    /// Log a performance measurement
    /// </summary>
    public static void LogPerformance(string operation, TimeSpan duration, string component = "")
    {
        var message = $"{operation} completed in {duration.TotalMilliseconds:F2}ms";
        var logMessage = FormatMessage("PERFORMANCE", message, component);
        WriteToLog(logMessage);
    }

    /// <summary>
    /// Log a debug message (only in debug builds)
    /// </summary>
    public static void LogDebug(string message, string component = "")
    {
#if DEBUG
        var logMessage = FormatMessage("DEBUG", message, component);
        WriteToLog(logMessage);
        Console.WriteLine(logMessage);
#endif
    }

    private static string FormatMessage(string level, string message, string component = "", Exception? exception = null)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var componentTag = string.IsNullOrEmpty(component) ? "" : $" [{component}]";
        var baseMessage = $"[{timestamp}] {level}{componentTag} {message}";
        
        if (exception != null)
        {
            baseMessage += $"\nException: {exception}";
        }
        
        return baseMessage;
    }

    private static void WriteToLog(string message)
    {
        try
        {
            Directory.CreateDirectory(LogsDirectory);
            File.AppendAllText(RuntimeLogFile, message + Environment.NewLine);
        }
        catch
        {
            // Ignore file logging errors to prevent infinite loops
        }
    }

    /// <summary>
    /// Initialize logging and create the logs directory
    /// </summary>
    public static void Initialize()
    {
        try
        {
            Directory.CreateDirectory(LogsDirectory);
            LogInfo("Application logger initialized", "LOGGER");
            LogInfo($"Runtime log file: {RuntimeLogFile}", "LOGGER");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize application logger: {ex.Message}");
        }
    }
}