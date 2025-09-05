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
            ApplicationLogger.LogInfo("MainWindowViewModel constructor called - Initializing commands...", "VIEWMODEL");
            
            InitializeCommand = ReactiveCommand.CreateFromTask(InitializeAsync);
            LoadMapCommand = ReactiveCommand.CreateFromTask(LoadMapAsync);
            SaveMapCommand = ReactiveCommand.CreateFromTask(SaveMapAsync);
            NewMapCommand = ReactiveCommand.CreateFromTask(NewMapAsync);
            UndoCommand = ReactiveCommand.CreateFromTask(UndoAsync, this.WhenAnyValue(x => x.CanUndo));
            RedoCommand = ReactiveCommand.CreateFromTask(RedoAsync, this.WhenAnyValue(x => x.CanRedo));
            
            // Initialize new dialog commands
            ShowJumpDialogCommand = ReactiveCommand.CreateFromTask(ShowJumpDialogAsync);
            ShowSetAnimationDialogCommand = ReactiveCommand.CreateFromTask(ShowSetAnimationDialogAsync);
            ShowSetDoorDialogCommand = ReactiveCommand.CreateFromTask(ShowSetDoorDialogAsync);
            ShowSetLightDialogCommand = ReactiveCommand.CreateFromTask(ShowSetLightDialogAsync);
            ShowAboutDialogCommand = ReactiveCommand.CreateFromTask(ShowAboutDialogAsync);
            
            Libraries = new ObservableCollection<LibraryItem>();
            
            ApplicationLogger.LogInfo("MainWindowViewModel constructor completed successfully", "VIEWMODEL");
        }
        catch (Exception ex)
        {
            ApplicationLogger.LogError("MainWindowViewModel constructor failed", ex, "VIEWMODEL");
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
    
    // New commands for dialogs from archived functionality
    public ReactiveCommand<Unit, Unit> ShowJumpDialogCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowSetAnimationDialogCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowSetDoorDialogCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowSetLightDialogCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowAboutDialogCommand { get; }

    private async Task InitializeAsync()
    {
        var startTime = DateTime.Now;
        try
        {
            ApplicationLogger.LogInfo("Initializing map editor...", "VIEWMODEL");
            StatusText = "Initializing...";
            
            var host = await Program.GetHostAsync();
            _libraryCatalog = host.Services.GetRequiredService<LibraryCatalog>();
            _mapReader = host.Services.GetRequiredService<MapReader>();
            _mapWriter = host.Services.GetRequiredService<MapWriter>();
            _editorService = host.Services.GetRequiredService<EditorService>();
            _logger = host.Services.GetRequiredService<ILogger<MainWindowViewModel>>();

            ApplicationLogger.LogInfo("Scanning for libraries...", "VIEWMODEL");
            StatusText = "Scanning for libraries...";
            
            var scanStartTime = DateTime.Now;
            await _libraryCatalog.ScanLibrariesAsync();
            ApplicationLogger.LogPerformance("Library scanning", DateTime.Now - scanStartTime, "VIEWMODEL");

            var wemadeMir2Libs = _libraryCatalog.GetLibrariesByType(LibraryType.WemadeMir2).ToList();
            var shandaMir2Libs = _libraryCatalog.GetLibrariesByType(LibraryType.ShandaMir2).ToList();
            var wemadeMir3Libs = _libraryCatalog.GetLibrariesByType(LibraryType.WemadeMir3).ToList();

            Libraries.Clear();
            foreach (var lib in wemadeMir2Libs.Concat(shandaMir2Libs).Concat(wemadeMir3Libs))
            {
                Libraries.Add(lib);
            }

            var totalLibs = wemadeMir2Libs.Count + shandaMir2Libs.Count + wemadeMir3Libs.Count;
            StatusText = $"Found {wemadeMir2Libs.Count} Wemade Mir2, {shandaMir2Libs.Count} Shanda Mir2, {wemadeMir3Libs.Count} Wemade Mir3 libraries";
            
            ApplicationLogger.LogInfo($"Found {totalLibs} total libraries ({wemadeMir2Libs.Count} Wemade Mir2, {shandaMir2Libs.Count} Shanda Mir2, {wemadeMir3Libs.Count} Wemade Mir3)", "VIEWMODEL");
            ApplicationLogger.LogPerformance("Map editor initialization", DateTime.Now - startTime, "VIEWMODEL");
            
            _logger?.LogInformation("Map editor initialized successfully");
            UpdateUndoRedoState();
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
            ApplicationLogger.LogError("Failed to initialize map editor", ex, "VIEWMODEL");
            _logger?.LogError(ex, "Failed to initialize map editor");
        }
    }

    private async Task LoadMapAsync()
    {
        var startTime = DateTime.Now;
        try
        {
            if (_mapReader == null || _editorService == null)
            {
                StatusText = "Please initialize first";
                ApplicationLogger.LogWarning("Attempted to load map before initialization", "VIEWMODEL");
                return;
            }

            ApplicationLogger.LogUsage("User initiated map loading", "VIEWMODEL");
            StatusText = "Loading map...";
            
            // TODO: Add file dialog
            var mapPath = "test_map.map";
            if (System.IO.File.Exists(mapPath))
            {
                ApplicationLogger.LogInfo($"Loading map from: {mapPath}", "VIEWMODEL");
                
                var map = await _mapReader.ReadAsync(mapPath);
                _editorService.CurrentMap = map;
                MapWidth = map.Width;
                MapHeight = map.Height;
                
                StatusText = $"Loaded map: {map.Width}x{map.Height} ({map.FormatType})";
                ApplicationLogger.LogInfo($"Successfully loaded map: {map.Width}x{map.Height}, format: {map.FormatType}", "VIEWMODEL");
                ApplicationLogger.LogPerformance("Map loading", DateTime.Now - startTime, "VIEWMODEL");
                
                UpdateUndoRedoState();
            }
            else
            {
                StatusText = "No test map found";
                ApplicationLogger.LogWarning($"Map file not found: {mapPath}", "VIEWMODEL");
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Error loading map: {ex.Message}";
            ApplicationLogger.LogError("Failed to load map", ex, "VIEWMODEL");
            _logger?.LogError(ex, "Failed to load map");
        }
    }

    private async Task SaveMapAsync()
    {
        var startTime = DateTime.Now;
        try
        {
            if (_mapWriter == null || _editorService == null)
            {
                StatusText = "Please initialize first";
                ApplicationLogger.LogWarning("Attempted to save map before initialization", "VIEWMODEL");
                return;
            }

            ApplicationLogger.LogUsage("User initiated map saving", "VIEWMODEL");
            StatusText = "Saving map...";
            
            var mapToSave = _editorService.CurrentMap;
            if (mapToSave == null)
            {
                // Create a test map for demonstration
                ApplicationLogger.LogInfo("No current map, creating test map for saving", "VIEWMODEL");
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
            ApplicationLogger.LogInfo($"Saving map to: {mapPath} (size: {mapToSave.Width}x{mapToSave.Height})", "VIEWMODEL");
            
            await _mapWriter.WriteAsync(mapToSave, mapPath);
            StatusText = $"Map saved to: {mapPath}";
            
            ApplicationLogger.LogInfo($"Successfully saved map to: {mapPath}", "VIEWMODEL");
            ApplicationLogger.LogPerformance("Map saving", DateTime.Now - startTime, "VIEWMODEL");
        }
        catch (Exception ex)
        {
            StatusText = $"Error saving map: {ex.Message}";
            ApplicationLogger.LogError("Failed to save map", ex, "VIEWMODEL");
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

    // Dialog methods from archived functionality
    private async Task ShowJumpDialogAsync()
    {
        try
        {
            var dialog = new Views.JumpDialog();
            // In a real implementation, we would show the dialog and handle the result
            // For now, just log the action
            StatusText = "Jump dialog opened";
            await Task.Delay(1);
        }
        catch (Exception ex)
        {
            StatusText = $"Error showing jump dialog: {ex.Message}";
            _logger?.LogError(ex, "Failed to show jump dialog");
        }
    }

    private async Task ShowSetAnimationDialogAsync()
    {
        try
        {
            var dialog = new Views.SetAnimationDialog();
            StatusText = "Set Animation dialog opened";
            await Task.Delay(1);
        }
        catch (Exception ex)
        {
            StatusText = $"Error showing set animation dialog: {ex.Message}";
            _logger?.LogError(ex, "Failed to show set animation dialog");
        }
    }

    private async Task ShowSetDoorDialogAsync()
    {
        try
        {
            var dialog = new Views.SetDoorDialog();
            StatusText = "Set Door dialog opened";
            await Task.Delay(1);
        }
        catch (Exception ex)
        {
            StatusText = $"Error showing set door dialog: {ex.Message}";
            _logger?.LogError(ex, "Failed to show set door dialog");
        }
    }

    private async Task ShowSetLightDialogAsync()
    {
        try
        {
            var dialog = new Views.SetLightDialog();
            StatusText = "Set Light dialog opened";
            await Task.Delay(1);
        }
        catch (Exception ex)
        {
            StatusText = $"Error showing set light dialog: {ex.Message}";
            _logger?.LogError(ex, "Failed to show set light dialog");
        }
    }

    private async Task ShowAboutDialogAsync()
    {
        try
        {
            var dialog = new Views.AboutDialog();
            StatusText = "About dialog opened";
            await Task.Delay(1);
        }
        catch (Exception ex)
        {
            StatusText = $"Error showing about dialog: {ex.Message}";
            _logger?.LogError(ex, "Failed to show about dialog");
        }
    }
}