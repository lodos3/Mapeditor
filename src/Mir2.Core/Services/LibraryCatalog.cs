using Mir2.Core.Models;
using System.Text.Json;

namespace Mir2.Core.Services;

/// <summary>
/// Manages the catalog of available libraries
/// </summary>
public class LibraryCatalog
{
    private readonly LibraryConfig _config;
    private readonly Dictionary<int, LibraryItem> _libraries = new();
    
    /// <summary>
    /// All available library items
    /// </summary>
    public IReadOnlyDictionary<int, LibraryItem> Libraries => _libraries;

    /// <summary>
    /// Configuration for library paths
    /// </summary>
    public LibraryConfig Config => _config;

    public LibraryCatalog()
    {
        _config = new LibraryConfig();
    }

    public LibraryCatalog(LibraryConfig config)
    {
        _config = config ?? new LibraryConfig();
    }

    /// <summary>
    /// Scans for available libraries based on configuration
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    public async Task ScanLibrariesAsync(CancellationToken cancellationToken = default)
    {
        _libraries.Clear();

        await Task.Run(() =>
        {
            // Scan Wemade Mir2 libraries (0-99)
            ScanWemadeMir2Libraries();
            
            // Scan Shanda Mir2 libraries (100-199)  
            ScanShandaMir2Libraries();
            
            // Scan Wemade Mir3 libraries (200-299)
            ScanWemadeMir3Libraries();
            
            // Scan Shanda Mir3 libraries (300-399)
            ScanShandaMir3Libraries();
            
        }, cancellationToken);
    }

    /// <summary>
    /// Gets library item by index
    /// </summary>
    /// <param name="index">Library index</param>
    /// <returns>Library item or null if not found</returns>
    public LibraryItem? GetLibrary(int index)
    {
        return _libraries.TryGetValue(index, out var library) ? library : null;
    }

    /// <summary>
    /// Gets libraries by type
    /// </summary>
    /// <param name="type">Library type</param>
    /// <returns>Collection of libraries of the specified type</returns>
    public IEnumerable<LibraryItem> GetLibrariesByType(LibraryType type)
    {
        return _libraries.Values.Where(lib => lib.Type == type);
    }

    /// <summary>
    /// Saves the configuration to a JSON file
    /// </summary>
    /// <param name="configPath">Path to save the config</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task SaveConfigAsync(string configPath, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(configPath, json, cancellationToken);
    }

    /// <summary>
    /// Loads configuration from a JSON file
    /// </summary>
    /// <param name="configPath">Path to load the config from</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Loaded configuration</returns>
    public static async Task<LibraryConfig> LoadConfigAsync(string configPath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(configPath))
            return new LibraryConfig();

        var json = await File.ReadAllTextAsync(configPath, cancellationToken);
        return JsonSerializer.Deserialize<LibraryConfig>(json) ?? new LibraryConfig();
    }

    private void ScanWemadeMir2Libraries()
    {
        var basePath = _config.LibraryPaths[LibraryType.WemadeMir2];
        if (!Directory.Exists(basePath)) return;

        // Standard libraries
        AddLibraryIfExists(basePath + "Tiles", 0, "Tiles", LibraryType.WemadeMir2);
        AddLibraryIfExists(basePath + "Smtiles", 1, "Smtiles", LibraryType.WemadeMir2);
        AddLibraryIfExists(basePath + "Objects", 2, "Objects", LibraryType.WemadeMir2);
        AddLibraryIfExists(basePath + "Objects_32bit", 90, "Objects_32bit", LibraryType.WemadeMir2);

        // Objects2-27
        for (int i = 2; i < 28; i++)
        {
            AddLibraryIfExists(basePath + $"Objects{i}", i + 1, $"Objects{i}", LibraryType.WemadeMir2);
        }
    }

    private void ScanShandaMir2Libraries()
    {
        var basePath = _config.LibraryPaths[LibraryType.ShandaMir2];
        if (!Directory.Exists(basePath)) return;

        // Tiles
        AddLibraryIfExists(basePath + "Tiles", 100, "Tiles", LibraryType.ShandaMir2);
        for (int i = 1; i < 10; i++)
        {
            AddLibraryIfExists(basePath + $"Tiles{i + 1}", 100 + i, $"Tiles{i + 1}", LibraryType.ShandaMir2);
        }

        // SmTiles
        AddLibraryIfExists(basePath + "SmTiles", 110, "SmTiles", LibraryType.ShandaMir2);
        for (int i = 1; i < 10; i++)
        {
            AddLibraryIfExists(basePath + $"SmTiles{i + 1}", 110 + i, $"SmTiles{i + 1}", LibraryType.ShandaMir2);
        }

        // Objects
        AddLibraryIfExists(basePath + "Objects", 120, "Objects", LibraryType.ShandaMir2);
        for (int i = 1; i < 79; i++)
        {
            AddLibraryIfExists(basePath + $"Objects{i + 1}", 120 + i, $"Objects{i + 1}", LibraryType.ShandaMir2);
        }

        // AniTiles
        AddLibraryIfExists(basePath + "AniTiles1", 199, "AniTiles1", LibraryType.ShandaMir2);
    }

    private void ScanWemadeMir3Libraries()
    {
        var basePath = _config.LibraryPaths[LibraryType.WemadeMir3];
        if (!Directory.Exists(basePath)) return;

        string[] mapStates = { "", "wood\\", "sand\\", "snow\\", "forest\\" };
        
        for (int i = 0; i < mapStates.Length; i++)
        {
            var path = basePath + mapStates[i];
            var statePrefix = string.IsNullOrEmpty(mapStates[i]) ? "" : mapStates[i].TrimEnd('\\') + "_";
            
            AddLibraryIfExists(path + "Tilesc", 200 + (i * 15), $"{statePrefix}Tilesc", LibraryType.WemadeMir3);
            AddLibraryIfExists(path + "Tiles30c", 201 + (i * 15), $"{statePrefix}Tiles30c", LibraryType.WemadeMir3);
            AddLibraryIfExists(path + "Tiles5c", 202 + (i * 15), $"{statePrefix}Tiles5c", LibraryType.WemadeMir3);
            AddLibraryIfExists(path + "Smtilesc", 203 + (i * 15), $"{statePrefix}Smtilesc", LibraryType.WemadeMir3);
            AddLibraryIfExists(path + "Housesc", 204 + (i * 15), $"{statePrefix}Housesc", LibraryType.WemadeMir3);
            AddLibraryIfExists(path + "Cliffsc", 205 + (i * 15), $"{statePrefix}Cliffsc", LibraryType.WemadeMir3);
            AddLibraryIfExists(path + "Dungeonsc", 206 + (i * 15), $"{statePrefix}Dungeonsc", LibraryType.WemadeMir3);
            AddLibraryIfExists(path + "Innersc", 207 + (i * 15), $"{statePrefix}Innersc", LibraryType.WemadeMir3);
            AddLibraryIfExists(path + "Furnituresc", 208 + (i * 15), $"{statePrefix}Furnituresc", LibraryType.WemadeMir3);
            AddLibraryIfExists(path + "Wallsc", 209 + (i * 15), $"{statePrefix}Wallsc", LibraryType.WemadeMir3);
            AddLibraryIfExists(path + "smObjectsc", 210 + (i * 15), $"{statePrefix}smObjectsc", LibraryType.WemadeMir3);
            AddLibraryIfExists(path + "Animationsc", 211 + (i * 15), $"{statePrefix}Animationsc", LibraryType.WemadeMir3);
            AddLibraryIfExists(path + "Object1c", 212 + (i * 15), $"{statePrefix}Object1c", LibraryType.WemadeMir3);
            AddLibraryIfExists(path + "Object2c", 213 + (i * 15), $"{statePrefix}Object2c", LibraryType.WemadeMir3);
        }
    }

    private void ScanShandaMir3Libraries()
    {
        // Placeholder for Shanda Mir3 libraries if needed
        var basePath = _config.LibraryPaths[LibraryType.ShandaMir3];
        if (!Directory.Exists(basePath)) return;
        
        // Add Shanda Mir3 specific library scanning logic here if required
    }

    private void AddLibraryIfExists(string basePath, int index, string name, LibraryType type)
    {
        var libPath = basePath + ".lib";
        if (!File.Exists(libPath)) return;

        var library = new LibraryItem
        {
            Name = name,
            Index = index,
            Type = type,
            FilePath = libPath,
            IsAvailable = true,
            ImageCount = 0 // This would be populated when the library is actually loaded
        };

        _libraries[index] = library;
    }
}