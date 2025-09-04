# Mir2 Map Editor - Unified Edition

A modern, cross-platform map editor for Mir2 game maps, consolidating functionality from multiple legacy solutions into a single, unified application.

## Features

### Core Functionality
- **Cross-Platform GUI**: Built with AvaloniaUI for Windows, macOS, and Linux
- **Modern Architecture**: .NET 8 with dependency injection, MVVM pattern, and ReactiveUI
- **Map Operations**: Create, load, and save map files with full format support
- **Library Management**: Automatic scanning and cataloging of Wemade Mir2, Shanda Mir2, and Mir3 libraries
- **Undo/Redo System**: Full undo/redo stack with unlimited levels
- **Map Editing**: Core map editing functionality with cell-level precision

### User Interface
- **Library Browser**: Browse and preview available texture libraries
- **Map Canvas**: Interactive map editing area with zoom and pan
- **Properties Panel**: Edit cell properties including light, layers, and animations
- **Tool Palette**: Select between different editing modes (select, paint, erase, fill)
- **Status Bar**: Real-time feedback on operations and current state

## Architecture

The solution is organized into several key projects:

### src/Mir2.Core
Core functionality including:
- **Models**: `MapData`, `CellInfo`, `LibraryItem`, etc.
- **Services**: `LibraryCatalog` for library management
- **I/O**: `MapReader` and `MapWriter` for file operations

### src/Mir2.Editor
The main GUI application featuring:
- **Views**: AvaloniaUI-based user interface
- **ViewModels**: MVVM pattern with ReactiveUI
- **Services**: `EditorService` for map editing operations

### src/Mir2.Render
Rendering functionality (future expansion for map visualization)

### tests/Mir2.Core.Tests
Comprehensive unit tests (29 tests, all passing)

## Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- Visual Studio 2022, VS Code, or JetBrains Rider

### Building the Application
```bash
# Clone the repository
git clone <repository-url>
cd Mapeditor

# Build the solution
dotnet build

# Run the application
dotnet run --project src/Mir2.Editor/Mir2.Editor.csproj
```

### Running Tests
```bash
dotnet test
```

## Usage

1. **Initialize the Application**: Click "Initialize" to scan for libraries
2. **Create New Map**: File → New Map to create a new map with custom dimensions
3. **Load Existing Map**: File → Load Map to open an existing map file
4. **Edit the Map**: Use the tools panel to select editing mode and modify cells
5. **Save Your Work**: File → Save Map to save changes

### Keyboard Shortcuts
- **Ctrl+Z**: Undo
- **Ctrl+Y**: Redo
- **Ctrl+N**: New Map
- **Ctrl+O**: Open Map
- **Ctrl+S**: Save Map

## Consolidation from Legacy Solutions

This unified edition consolidates functionality from multiple previous solutions:
- **Legacy Windows Forms Editor**: Core editing functionality, undo/redo system
- **Custom UI Components**: Integrated into modern UI framework
- **Multiple Platform Versions**: Now unified for cross-platform compatibility

The `_DEL_ARCHIVED` directory contains the original legacy solutions for reference.

## Configuration

The application uses `library_config.json` to store library paths and settings:
```json
{
  "LibraryPaths": {
    "WemadeMir2": ".\\Data\\Map\\WemadeMir2\\",
    "ShandaMir2": ".\\Data\\Map\\ShandaMir2\\",
    "WemadeMir3": ".\\Data\\Map\\WemadeMir3\\",
    "ShandaMir3": ".\\Data\\Map\\ShandaMir3\\"
  },
  "ObjectsPath": ".\\Data\\Objects\\",
  "AutoScanOnStartup": true
}
```

## Contributing

This project follows modern .NET development practices:
- SOLID principles
- Clean Architecture
- Comprehensive testing
- Cross-platform compatibility

## License

[Add appropriate license information]

## Version History

### v2.0.0 - Unified Edition
- Consolidated multiple solutions into single cross-platform application
- Modern AvaloniaUI interface
- Complete undo/redo system
- Enhanced library management
- Cross-platform compatibility (Windows, macOS, Linux)

### Legacy Versions
- Original Windows Forms versions archived in `_DEL_ARCHIVED`