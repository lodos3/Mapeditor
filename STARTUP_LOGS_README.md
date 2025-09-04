# Map Editor Startup and Error Logging

## Overview

The Map Editor now includes comprehensive startup and error logging to help diagnose issues when the application fails to start.

## Log Location

Startup logs are automatically created in the following location:
- **Windows**: `%LOCALAPPDATA%\Mir2Editor\Logs\`
- **Linux/macOS**: `~/.local/share/Mir2Editor/Logs/`

Log files are named with timestamps: `startup_yyyyMMdd_HHmmss.log`

## What Gets Logged

The application logs the following information:

### Startup Process
- Application start time and environment information
- Command line arguments
- Current directory and runtime version
- Platform information
- Avalonia framework initialization steps
- Main window creation process
- View model initialization

### Error Information
- Complete exception details with stack traces
- Timestamps for all events
- Error location (Program, App, MainWindow, or ViewModel)

## Viewing Logs

### Console Output
All log messages are also written to the console/terminal for immediate viewing.

### Log Files
- Each application startup creates a new log file
- Log files contain complete startup sequence
- Errors include full exception details and stack traces

## Common Issues and Solutions

### XOpenDisplay Failed Error
This error occurs when running the GUI application in a headless environment (no display server):
```
FATAL ERROR during application startup: System.Exception: XOpenDisplay failed
```
**Solution**: Run the application with a display server or use remote desktop/VNC.

### File Permission Errors
If log files cannot be created due to permission issues, errors will still be shown in console output.

## Troubleshooting Steps

1. **Check Console Output**: Run the application from command line to see immediate error messages
2. **Check Log Files**: Navigate to the logs directory to view detailed startup logs
3. **Look for Patterns**: Multiple log files can help identify consistent failure points
4. **Environment Issues**: Check if display server, dependencies, or permissions are the cause

## Example Log Output

```
[2024-01-20 15:30:45.123] === Map Editor Startup Log ===
[2024-01-20 15:30:45.124] Application starting at: 1/20/2024 3:30:45 PM
[2024-01-20 15:30:45.125] Command line args: 
[2024-01-20 15:30:45.126] Current directory: /path/to/application
[2024-01-20 15:30:45.127] Runtime version: 8.0.0
[2024-01-20 15:30:45.128] Platform: Unix 6.2.0.0
[2024-01-20 15:30:45.129] Building Avalonia application...
[2024-01-20 15:30:45.150] [APP] App.Initialize() called - Loading XAML...
[2024-01-20 15:30:45.200] FATAL ERROR during application startup: System.Exception: XOpenDisplay failed
```

This logging system provides comprehensive debugging information for all startup failures.