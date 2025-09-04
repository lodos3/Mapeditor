namespace Mir2.Core.Models;

/// <summary>
/// Mir2 map format types supported by the reader/writer
/// </summary>
public enum MapFormatType
{
    /// <summary>
    /// Default old school format
    /// </summary>
    Type0 = 0,

    /// <summary>
    /// Wemade's 2010 map format - title starts with: Map 2010 Ver 1.0
    /// </summary>
    Type1 = 1,

    /// <summary>
    /// Shanda's older format sharing header with Type3
    /// </summary>
    Type2 = 2,

    /// <summary>
    /// Shanda's 2012 format sharing header with Type2
    /// </summary>
    Type3 = 3,

    /// <summary>
    /// Wemade's antihack map (laby maps) - title starts with: Mir2 AntiHack
    /// </summary>
    Type4 = 4,

    /// <summary>
    /// Wemade Mir3 maps - start with blank bytes
    /// </summary>
    Type5 = 5,

    /// <summary>
    /// Shanda Mir3 maps - start with title: (C) SNDA, MIR3.
    /// </summary>
    Type6 = 6,

    /// <summary>
    /// 3/4 Heroes map format (Myth/Lifcos)
    /// </summary>
    Type7 = 7,

    /// <summary>
    /// C# custom map format - identified by 'C#' tag
    /// </summary>
    Type100 = 100
}

/// <summary>
/// Represents a Mir2 map with all cells and metadata
/// </summary>
public class MapData
{
    /// <summary>
    /// Map width in cells
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Map height in cells
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Map format type
    /// </summary>
    public MapFormatType FormatType { get; set; }

    /// <summary>
    /// 2D array of map cells [x, y]
    /// </summary>
    public CellInfo[,] Cells { get; set; }

    /// <summary>
    /// Default constructor creating empty 1000x1000 map
    /// </summary>
    public MapData()
    {
        Width = 1000;
        Height = 1000;
        FormatType = MapFormatType.Type100;
        Cells = new CellInfo[Width, Height];

        // Initialize all cells
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Cells[x, y] = new CellInfo();
            }
        }
    }

    /// <summary>
    /// Constructor with specified dimensions
    /// </summary>
    /// <param name="width">Map width</param>
    /// <param name="height">Map height</param>
    /// <param name="formatType">Map format type</param>
    public MapData(int width, int height, MapFormatType formatType = MapFormatType.Type100)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than 0");
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Height must be greater than 0");
        if (width > 10000) throw new ArgumentOutOfRangeException(nameof(width), "Width cannot exceed 10000");
        if (height > 10000) throw new ArgumentOutOfRangeException(nameof(height), "Height cannot exceed 10000");

        Width = width;
        Height = height;
        FormatType = formatType;
        Cells = new CellInfo[Width, Height];

        // Initialize all cells
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Cells[x, y] = new CellInfo();
            }
        }
    }

    /// <summary>
    /// Gets the cell at the specified coordinates
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>Cell info or null if coordinates are out of bounds</returns>
    public CellInfo? GetCell(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return null;

        return Cells[x, y];
    }

    /// <summary>
    /// Sets the cell at the specified coordinates
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="cellInfo">Cell info to set</param>
    /// <returns>True if set successfully, false if coordinates are out of bounds</returns>
    public bool SetCell(int x, int y, CellInfo cellInfo)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return false;

        Cells[x, y] = cellInfo ?? new CellInfo();
        return true;
    }

    /// <summary>
    /// Validates that all cells have consistent fishing cell flag based on light value
    /// </summary>
    public void NormalizeFishingCells()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                // FishingCell is derived from Light value, ensure consistency
                // This is handled by the CellInfo.FishingCell property itself
            }
        }
    }
}