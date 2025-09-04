namespace Mir2.Core.Models;

/// <summary>
/// Represents a library item in the catalog
/// </summary>
public class LibraryItem
{
    /// <summary>
    /// Display name of the library
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Library index/value
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Library version/type (Wemade Mir2, Shanda Mir2, Wemade Mir3, etc.)
    /// </summary>
    public LibraryType Type { get; set; }

    /// <summary>
    /// File path to the library
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Whether the library is available/loaded
    /// </summary>
    public bool IsAvailable { get; set; }

    /// <summary>
    /// Number of images in the library
    /// </summary>
    public int ImageCount { get; set; }

    public override string ToString() => Name;
}

/// <summary>
/// Library types supported by the editor
/// </summary>
public enum LibraryType
{
    /// <summary>
    /// Wemade Mir2 libraries (indices 0-99)
    /// </summary>
    WemadeMir2 = 0,

    /// <summary>
    /// Shanda Mir2 libraries (indices 100-199)
    /// </summary>
    ShandaMir2 = 1,

    /// <summary>
    /// Wemade Mir3 libraries (indices 200-299)
    /// </summary>
    WemadeMir3 = 2,

    /// <summary>
    /// Shanda Mir3 libraries (indices 300-399)
    /// </summary>
    ShandaMir3 = 3
}

/// <summary>
/// Configuration for library scanning
/// </summary>
public class LibraryConfig
{
    /// <summary>
    /// Root paths for different library types
    /// </summary>
    public Dictionary<LibraryType, string> LibraryPaths { get; set; } = new()
    {
        { LibraryType.WemadeMir2, @".\Data\Map\WemadeMir2\" },
        { LibraryType.ShandaMir2, @".\Data\Map\ShandaMir2\" },
        { LibraryType.WemadeMir3, @".\Data\Map\WemadeMir3\" },
        { LibraryType.ShandaMir3, @".\Data\Map\ShandaMir3\" }
    };

    /// <summary>
    /// Objects path for .X files
    /// </summary>
    public string ObjectsPath { get; set; } = @".\Data\Objects\";

    /// <summary>
    /// Whether to scan libraries on startup
    /// </summary>
    public bool AutoScanOnStartup { get; set; } = true;
}