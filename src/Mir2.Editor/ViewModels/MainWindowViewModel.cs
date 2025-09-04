using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using Mir2.Core.Services;
using Mir2.Core.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using Mir2.Core.Models;
using System.Linq;
using Mir2.Editor.Services;
using System;
using System.IO;

namespace Mir2.Editor.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private static readonly string LogsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mir2Editor", "Logs");
    private static readonly string StartupLogFile = Path.Combine(LogsDirectory, $"startup_{DateTime.Now:yyyyMMdd_HHmmss}.log");
    
    private string _title = "Mir2 Map Editor - Unified Edition";
    private string _statusText = "Ready";
    private LibraryCatalog? _libraryCatalog;
    private MapReader? _mapReader;
    private MapWriter? _mapWriter;
    private EditorService? _editorService;
    private ILogger<MainWindowViewModel>? _logger;
    
    private bool _canUndo = false;
    private bool _canRedo = false;
    private int _mapWidth = 0;
    private int _mapHeight = 0;

    public MainWindowViewModel()
    {
        try
        {
            LogStartup("MainWindowViewModel constructor called - Initializing commands...");
            
            InitializeCommand = ReactiveCommand.CreateFromTask(InitializeAsync);
            LoadMapCommand = ReactiveCommand.CreateFromTask(LoadMapAsync);
            SaveMapCommand = ReactiveCommand.CreateFromTask(SaveMapAsync);
            NewMapCommand = ReactiveCommand.CreateFromTask(NewMapAsync);
            UndoCommand = ReactiveCommand.CreateFromTask(UndoAsync, this.WhenAnyValue(x => x.CanUndo));
            RedoCommand = ReactiveCommand.CreateFromTask(RedoAsync, this.WhenAnyValue(x => x.CanRedo));
            
            Libraries = new ObservableCollection<LibraryItem>();
            
            LogStartup("MainWindowViewModel constructor completed successfully");
        }
        catch (Exception ex)
        {
            LogStartup($"ERROR in MainWindowViewModel constructor: {ex}");
            throw;
        }
    }

    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => this.RaiseAndSetIfChanged(ref _statusText, value);
    }

    public bool CanUndo
    {
        get => _canUndo;
        set => this.RaiseAndSetIfChanged(ref _canUndo, value);
    }

    public bool CanRedo
    {
        get => _canRedo;
        set => this.RaiseAndSetIfChanged(ref _canRedo, value);
    }

    public int MapWidth
    {
        get => _mapWidth;
        set => this.RaiseAndSetIfChanged(ref _mapWidth, value);
    }

    public int MapHeight
    {
        get => _mapHeight;
        set => this.RaiseAndSetIfChanged(ref _mapHeight, value);
    }

    public ObservableCollection<LibraryItem> Libraries { get; }

    public ReactiveCommand<Unit, Unit> InitializeCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadMapCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveMapCommand { get; }
    public ReactiveCommand<Unit, Unit> NewMapCommand { get; }
    public ReactiveCommand<Unit, Unit> UndoCommand { get; }
    public ReactiveCommand<Unit, Unit> RedoCommand { get; }

    private async Task InitializeAsync()
    {
        try
        {
            StatusText = "Initializing...";
            
            var host = await Program.GetHostAsync();
            _libraryCatalog = host.Services.GetRequiredService<LibraryCatalog>();
            _mapReader = host.Services.GetRequiredService<MapReader>();
            _mapWriter = host.Services.GetRequiredService<MapWriter>();
            _editorService = host.Services.GetRequiredService<EditorService>();
            _logger = host.Services.GetRequiredService<ILogger<MainWindowViewModel>>();

            StatusText = "Scanning for libraries...";
            await _libraryCatalog.ScanLibrariesAsync();

            var wemadeMir2Libs = _libraryCatalog.GetLibrariesByType(LibraryType.WemadeMir2).ToList();
            var shandaMir2Libs = _libraryCatalog.GetLibrariesByType(LibraryType.ShandaMir2).ToList();
            var wemadeMir3Libs = _libraryCatalog.GetLibrariesByType(LibraryType.WemadeMir3).ToList();

            Libraries.Clear();
            foreach (var lib in wemadeMir2Libs.Concat(shandaMir2Libs).Concat(wemadeMir3Libs))
            {
                Libraries.Add(lib);
            }

            StatusText = $"Found {wemadeMir2Libs.Count} Wemade Mir2, {shandaMir2Libs.Count} Shanda Mir2, {wemadeMir3Libs.Count} Wemade Mir3 libraries";
            
            _logger?.LogInformation("Map editor initialized successfully");
            UpdateUndoRedoState();
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
            _logger?.LogError(ex, "Failed to initialize map editor");
        }
    }

    private async Task LoadMapAsync()
    {
        try
        {
            if (_mapReader == null || _editorService == null)
            {
                StatusText = "Please initialize first";
                return;
            }

            StatusText = "Loading map...";
            // TODO: Add file dialog
            var mapPath = "test_map.map";
            if (System.IO.File.Exists(mapPath))
            {
                var map = await _mapReader.ReadAsync(mapPath);
                _editorService.CurrentMap = map;
                MapWidth = map.Width;
                MapHeight = map.Height;
                StatusText = $"Loaded map: {map.Width}x{map.Height} ({map.FormatType})";
                UpdateUndoRedoState();
            }
            else
            {
                StatusText = "No test map found";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Error loading map: {ex.Message}";
            _logger?.LogError(ex, "Failed to load map");
        }
    }

    private async Task SaveMapAsync()
    {
        try
        {
            if (_mapWriter == null || _editorService == null)
            {
                StatusText = "Please initialize first";
                return;
            }

            StatusText = "Saving map...";
            
            var mapToSave = _editorService.CurrentMap;
            if (mapToSave == null)
            {
                // Create a test map for demonstration
                mapToSave = new MapData(50, 50);
                mapToSave.Cells[0, 0] = new CellInfo
                {
                    BackIndex = 0,
                    BackImage = 1,
                    Light = 50
                };
                _editorService.CurrentMap = mapToSave;
                MapWidth = mapToSave.Width;
                MapHeight = mapToSave.Height;
            }

            var mapPath = "test_map.map";
            await _mapWriter.WriteAsync(mapToSave, mapPath);
            StatusText = $"Map saved to: {mapPath}";
        }
        catch (Exception ex)
        {
            StatusText = $"Error saving map: {ex.Message}";
            _logger?.LogError(ex, "Failed to save map");
        }
    }

    private async Task NewMapAsync()
    {
        try
        {
            if (_editorService == null)
            {
                StatusText = "Please initialize first";
                return;
            }

            var dialog = new Mir2.Editor.Views.NewMapDialog();
            
            // In a real application, we would show the dialog properly
            // For now, create a new map with default values
            StatusText = "Creating new map...";
            var newMap = new MapData(dialog.ViewModel?.Width ?? 100, dialog.ViewModel?.Height ?? 100);
            _editorService.CurrentMap = newMap;
            MapWidth = newMap.Width;
            MapHeight = newMap.Height;
            StatusText = $"Created new map: {newMap.Width}x{newMap.Height}";
            UpdateUndoRedoState();
            await Task.Delay(100);
        }
        catch (Exception ex)
        {
            StatusText = $"Error creating new map: {ex.Message}";
            _logger?.LogError(ex, "Failed to create new map");
        }
    }

    private async Task UndoAsync()
    {
        try
        {
            if (_editorService == null)
                return;

            var changes = _editorService.Undo();
            if (changes != null)
            {
                StatusText = $"Undone {changes.Length} cell changes";
                UpdateUndoRedoState();
            }
            await Task.Delay(1);
        }
        catch (Exception ex)
        {
            StatusText = $"Error during undo: {ex.Message}";
            _logger?.LogError(ex, "Failed to undo");
        }
    }

    private async Task RedoAsync()
    {
        try
        {
            if (_editorService == null)
                return;

            var changes = _editorService.Redo();
            if (changes != null)
            {
                StatusText = $"Redone {changes.Length} cell changes";
                UpdateUndoRedoState();
            }
            await Task.Delay(1);
        }
        catch (Exception ex)
        {
            StatusText = $"Error during redo: {ex.Message}";
            _logger?.LogError(ex, "Failed to redo");
        }
    }

    private void UpdateUndoRedoState()
    {
        if (_editorService != null)
        {
            CanUndo = _editorService.UndoCount > 0;
            CanRedo = _editorService.RedoCount > 0;
        }
    }

    private static void LogStartup(string message)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var logMessage = $"[{timestamp}] [VIEWMODEL] {message}";
        
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