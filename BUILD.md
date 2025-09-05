# Build Instructions for Mir2 Map Editor

## Overview

The Mir2 Map Editor is built using .NET 8 and can be compiled as a single self-contained executable, eliminating the need for scattered DLL files.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- Windows 10/11 (for Windows builds) or Linux x64 (for Linux builds)
- Git (for cloning the repository)

## Quick Build

### Windows

```bash
# Clone the repository
git clone https://github.com/lodos3/Mapeditor.git
cd Mapeditor

# Build single-file executable
build-windows.bat
```

The executable will be created at `dist/windows/Mir2.Editor.exe`

### Linux

```bash
# Clone the repository
git clone https://github.com/lodos3/Mapeditor.git
cd Mapeditor

# Build single-file executable
chmod +x build-linux.sh
./build-linux.sh
```

The executable will be created at `dist/linux/Mir2.Editor`

## Manual Build Commands

### Windows x64
```bash
cd src/Mir2.Editor
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

### Linux x64
```bash
cd src/Mir2.Editor
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true
```

### Other Platforms
The application can be built for other platforms supported by .NET 8. Replace the `-r` parameter with your target runtime identifier:

- `win-arm64` - Windows ARM64
- `osx-x64` - macOS x64
- `osx-arm64` - macOS ARM64 (Apple Silicon)

## Development Build

For development builds with debugging symbols:

```bash
dotnet build
```

## Running Tests

```bash
dotnet test
```

## Project Structure

```
src/
├── Mir2.Core/          # Core map reading/writing functionality
├── Mir2.Editor/        # Main GUI application (Avalonia UI)
└── Mir2.Render/        # Rendering and graphics utilities

tests/
└── Mir2.Core.Tests/    # Unit tests for core functionality
```

## Build Configuration

The project is configured for single-file publishing in `src/Mir2.Editor/Mir2.Editor.csproj`:

- `PublishSingleFile=true` - Packages everything into one executable
- `SelfContained=true` - Includes the .NET runtime
- `IncludeNativeLibrariesForSelfExtract=true` - Includes native dependencies

## Supported Map Formats

The editor supports the following map format types:

- **Type 0**: Default old school format
- **Type 1**: Wemade's 2010 format ("Map 2010 Ver 1.0")
- **Type 2**: Shanda's older format
- **Type 3**: Shanda's 2012 format
- **Type 4**: Wemade's antihack format ("Mir2 AntiHack")
- **Type 5**: Wemade Mir3 format (starts with blank bytes)
- **Type 6**: Shanda Mir3 format ("(C) SNDA, MIR3.")
- **Type 7**: 3/4 Heroes format (Myth/Lifcos)
- **Type 100**: C# custom format (identified by 'C#' tag)

## Distribution

The single-file executable includes everything needed to run the application:

- All .NET dependencies
- Application assemblies
- Native libraries

**Note**: Debug symbol files (*.pdb) can be safely deleted from the distribution to reduce file size.

## Troubleshooting

### Build Fails
- Ensure .NET 8 SDK is properly installed
- Check that all NuGet packages are restored: `dotnet restore`
- Clean and rebuild: `dotnet clean && dotnet build`

### Runtime Issues
- The single-file executable should run without requiring .NET to be installed on the target machine
- If you encounter missing dependencies, ensure `SelfContained=true` is set in the project file

## Legacy Code

The `_DEL_ARCHIVED/` directory contains the original legacy implementation for reference. The modern implementation in the `src/` directory provides the same functionality with improved architecture and testing.