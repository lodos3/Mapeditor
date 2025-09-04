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

namespace Mir2.Editor.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private string _title = "Crystal Mir2 Map Editor";
    private string _statusText = "Ready";
    private LibraryCatalog? _libraryCatalog;
    private MapReader? _mapReader;
    private MapWriter? _mapWriter;
    private ILogger<MainWindowViewModel>? _logger;

    public MainWindowViewModel()
    {
        InitializeCommand = ReactiveCommand.CreateFromTask(InitializeAsync);
        LoadMapCommand = ReactiveCommand.CreateFromTask(LoadMapAsync);
        SaveMapCommand = ReactiveCommand.CreateFromTask(SaveMapAsync);
        NewMapCommand = ReactiveCommand.CreateFromTask(NewMapAsync);
        
        Libraries = new ObservableCollection<LibraryItem>();
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

    public ObservableCollection<LibraryItem> Libraries { get; }

    public ReactiveCommand<Unit, Unit> InitializeCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadMapCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveMapCommand { get; }
    public ReactiveCommand<Unit, Unit> NewMapCommand { get; }

    private async Task InitializeAsync()
    {
        try
        {
            StatusText = "Initializing...";
            
            var host = await Program.GetHostAsync();
            _libraryCatalog = host.Services.GetRequiredService<LibraryCatalog>();
            _mapReader = host.Services.GetRequiredService<MapReader>();
            _mapWriter = host.Services.GetRequiredService<MapWriter>();
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
            if (_mapReader == null)
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
                StatusText = $"Loaded map: {map.Width}x{map.Height} ({map.FormatType})";
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
            if (_mapWriter == null)
            {
                StatusText = "Please initialize first";
                return;
            }

            StatusText = "Saving map...";
            // Create a test map for demonstration
            var testMap = new MapData(50, 50);
            testMap.Cells[0, 0] = new CellInfo
            {
                BackIndex = 0,
                BackImage = 1,
                Light = 50
            };

            var mapPath = "test_map.map";
            await _mapWriter.WriteAsync(testMap, mapPath);
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
            StatusText = "Creating new map...";
            // TODO: Add new map dialog
            await Task.Delay(100);
            StatusText = "Ready for new map creation";
        }
        catch (Exception ex)
        {
            StatusText = $"Error creating new map: {ex.Message}";
            _logger?.LogError(ex, "Failed to create new map");
        }
    }
}