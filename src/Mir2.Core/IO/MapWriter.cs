using Mir2.Core.Models;

namespace Mir2.Core.IO;

/// <summary>
/// Writes Mir2 map files in Type100 (C# custom) format
/// </summary>
public class MapWriter
{
    /// <summary>
    /// Writes a map to the specified file path in Type100 format
    /// </summary>
    /// <param name="mapData">Map data to write</param>
    /// <param name="filePath">File path to write to</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task WriteAsync(MapData mapData, string filePath, CancellationToken cancellationToken = default)
    {
        var bytes = WriteToBytes(mapData);
        await File.WriteAllBytesAsync(filePath, bytes, cancellationToken);
    }

    /// <summary>
    /// Writes a map to a byte array in Type100 format
    /// </summary>
    /// <param name="mapData">Map data to write</param>
    /// <returns>Byte array containing the map data</returns>
    public byte[] WriteToBytes(MapData mapData)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream);

        // Write header: version (2 bytes) + 'C#' tag (2 bytes)
        writer.Write((short)1);  // Version 1
        writer.Write('C');       // C# tag part 1
        writer.Write('#');       // C# tag part 2

        // Write map dimensions
        writer.Write((short)mapData.Width);
        writer.Write((short)mapData.Height);

        // Write cell data
        for (int x = 0; x < mapData.Width; x++)
        {
            for (int y = 0; y < mapData.Height; y++)
            {
                var cell = mapData.Cells[x, y];
                
                writer.Write(cell.BackIndex);
                writer.Write(cell.BackImage);        // 4 bytes for BackImage in Type100
                writer.Write(cell.MiddleIndex);
                writer.Write(cell.MiddleImage);
                writer.Write(cell.FrontIndex);
                writer.Write(cell.FrontImage);
                writer.Write(cell.DoorIndex);
                writer.Write(cell.DoorOffset);
                writer.Write(cell.FrontAnimationFrame);
                writer.Write(cell.FrontAnimationTick);
                writer.Write(cell.MiddleAnimationFrame);
                writer.Write(cell.MiddleAnimationTick);
                writer.Write(cell.TileAnimationImage);
                writer.Write(cell.TileAnimationOffset);
                writer.Write(cell.TileAnimationFrames);
                writer.Write(cell.Light);
            }
        }

        writer.Flush();
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Synchronous version of WriteAsync
    /// </summary>
    /// <param name="mapData">Map data to write</param>
    /// <param name="filePath">File path to write to</param>
    public void Write(MapData mapData, string filePath)
    {
        WriteAsync(mapData, filePath).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Calculates the expected file size for a map in Type100 format
    /// </summary>
    /// <param name="width">Map width</param>
    /// <param name="height">Map height</param>
    /// <returns>Expected file size in bytes</returns>
    public static long CalculateFileSize(int width, int height)
    {
        // Header: 8 bytes (version + tag + dimensions)
        // Each cell: 26 bytes (not 28 as originally calculated)
        return 8 + (width * height * 26);
    }

    /// <summary>
    /// Validates that the map data can be saved
    /// </summary>
    /// <param name="mapData">Map data to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool ValidateMapData(MapData mapData)
    {
        if (mapData == null) return false;
        if (mapData.Width <= 0 || mapData.Height <= 0) return false;
        if (mapData.Width > 10000 || mapData.Height > 10000) return false; // Reasonable limits
        if (mapData.Cells == null) return false;
        if (mapData.Cells.GetLength(0) != mapData.Width || mapData.Cells.GetLength(1) != mapData.Height) return false;

        return true;
    }
}