using Mir2.Core.Models;

namespace Mir2.Core.IO;

/// <summary>
/// Reads Mir2 map files in various formats (Types 0-7, 100)
/// </summary>
public class MapReader
{
    /// <summary>
    /// Reads a map file from the specified path
    /// </summary>
    /// <param name="filePath">Path to the map file</param>
    /// <returns>MapData containing the loaded map</returns>
    /// <exception cref="FileNotFoundException">Thrown when file doesn't exist</exception>
    /// <exception cref="InvalidDataException">Thrown when map format is invalid</exception>
    public async Task<MapData> ReadAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            // Return default empty map if file doesn't exist
            return new MapData();
        }

        byte[] bytes = await File.ReadAllBytesAsync(filePath);
        return ReadFromBytes(bytes);
    }

    /// <summary>
    /// Reads a map from a byte array
    /// </summary>
    /// <param name="bytes">Map file bytes</param>
    /// <returns>MapData containing the loaded map</returns>
    /// <exception cref="InvalidDataException">Thrown when map format is invalid</exception>
    public MapData ReadFromBytes(byte[] bytes)
    {
        if (bytes.Length == 0)
            return new MapData();

        var formatType = DetectMapFormat(bytes);
        
        return formatType switch
        {
            MapFormatType.Type100 => LoadMapType100(bytes),
            MapFormatType.Type5 => LoadMapType5(bytes),
            MapFormatType.Type6 => LoadMapType6(bytes),
            MapFormatType.Type4 => LoadMapType4(bytes),
            MapFormatType.Type1 => LoadMapType1(bytes),
            MapFormatType.Type3 => LoadMapType3(bytes),
            MapFormatType.Type2 => LoadMapType2(bytes),
            MapFormatType.Type7 => LoadMapType7(bytes),
            _ => LoadMapType0(bytes)
        };
    }

    /// <summary>
    /// Synchronous version of ReadAsync
    /// </summary>
    /// <param name="filePath">Path to the map file</param>
    /// <returns>MapData containing the loaded map</returns>
    public MapData Read(string filePath)
    {
        return ReadAsync(filePath).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Detects the map format type from the file header
    /// </summary>
    /// <param name="bytes">Map file bytes</param>
    /// <returns>Detected map format type</returns>
    private static MapFormatType DetectMapFormat(byte[] bytes)
    {
        if (bytes.Length < 20) return MapFormatType.Type0;

        // C# custom map format (Type100)
        if (bytes[2] == 0x43 && bytes[3] == 0x23)
            return MapFormatType.Type100;

        // Wemade mir3 maps have no title they just start with blank bytes (Type5)
        if (bytes[0] == 0)
            return MapFormatType.Type5;

        // Shanda mir3 maps start with title: (C) SNDA, MIR3. (Type6)
        if (bytes[0] == 0x0F && bytes[5] == 0x53 && bytes[14] == 0x33)
            return MapFormatType.Type6;

        // Wemades antihack map (laby maps) title start with: Mir2 AntiHack (Type4)
        if (bytes[0] == 0x15 && bytes[4] == 0x32 && bytes[6] == 0x41 && bytes[19] == 0x31)
            return MapFormatType.Type4;

        // Wemades 2010 map format title starts with: Map 2010 Ver 1.0 (Type1)
        if (bytes[0] == 0x10 && bytes[2] == 0x61 && bytes[7] == 0x31 && bytes[14] == 0x31)
            return MapFormatType.Type1;

        // Shanda's 2012 format and one of shandas(wemades) older formats share same header info (Type2/Type3)
        if (bytes[4] == 0x0F && bytes[18] == 0x0D && bytes[19] == 0x0A)
        {
            int w = bytes[0] + (bytes[1] << 8);
            int h = bytes[2] + (bytes[3] << 8);
            
            // Check file size to distinguish Type2 from Type3
            return bytes.Length > (52 + (w * h * 14)) ? MapFormatType.Type3 : MapFormatType.Type2;
        }

        // 3/4 heroes map format (myth/lifcos) (Type7)
        if (bytes[0] == 0x0D && bytes[1] == 0x4C && bytes[7] == 0x20 && bytes[11] == 0x6D)
            return MapFormatType.Type7;

        // Default old school format
        return MapFormatType.Type0;
    }

    /// <summary>
    /// Loads C# custom map format (Type100)
    /// </summary>
    private static MapData LoadMapType100(byte[] bytes)
    {
        try
        {
            int offset = 4;
            
            // Only support version 1
            if (bytes[0] != 1 || bytes[1] != 0) 
                throw new InvalidDataException("Unsupported map version");

            int width = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            int height = BitConverter.ToInt16(bytes, offset);
            
            var mapData = new MapData(width, height, MapFormatType.Type100);
            offset = 8;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var cell = new CellInfo();
                    
                    cell.BackIndex = BitConverter.ToInt16(bytes, offset);
                    offset += 2;
                    
                    cell.BackImage = BitConverter.ToInt32(bytes, offset);
                    offset += 4;
                    
                    cell.MiddleIndex = BitConverter.ToInt16(bytes, offset);
                    offset += 2;
                    cell.MiddleImage = BitConverter.ToInt16(bytes, offset);
                    offset += 2;
                    cell.FrontIndex = BitConverter.ToInt16(bytes, offset);
                    offset += 2;
                    cell.FrontImage = BitConverter.ToInt16(bytes, offset);
                    offset += 2;
                    
                    cell.DoorIndex = bytes[offset++];
                    cell.DoorOffset = bytes[offset++];
                    cell.FrontAnimationFrame = bytes[offset++];
                    cell.FrontAnimationTick = bytes[offset++];
                    cell.MiddleAnimationFrame = bytes[offset++];
                    cell.MiddleAnimationTick = bytes[offset++];
                    
                    cell.TileAnimationImage = BitConverter.ToInt16(bytes, offset);
                    offset += 2;
                    cell.TileAnimationOffset = BitConverter.ToInt16(bytes, offset);
                    offset += 2;
                    cell.TileAnimationFrames = bytes[offset++];
                    cell.Light = bytes[offset++];

                    mapData.Cells[x, y] = cell;
                }
            }

            return mapData;
        }
        catch (Exception ex)
        {
            throw new InvalidDataException($"Error loading Type100 map: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Loads default old school format (Type0)
    /// </summary>
    private static MapData LoadMapType0(byte[] bytes)
    {
        try
        {
            int offset = 0;
            int width = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            int height = BitConverter.ToInt16(bytes, offset);
            
            var mapData = new MapData(width, height, MapFormatType.Type0);
            offset = 52;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var cell = new CellInfo();
                    
                    cell.BackIndex = BitConverter.ToInt16(bytes, offset);
                    offset += 2;
                    cell.BackImage = BitConverter.ToInt16(bytes, offset);
                    offset += 2;
                    cell.MiddleIndex = BitConverter.ToInt16(bytes, offset);
                    offset += 2;
                    cell.MiddleImage = BitConverter.ToInt16(bytes, offset);
                    offset += 2;
                    cell.FrontIndex = BitConverter.ToInt16(bytes, offset);
                    offset += 2;
                    cell.FrontImage = BitConverter.ToInt16(bytes, offset);
                    offset += 2;
                    
                    cell.DoorIndex = bytes[offset++];
                    cell.DoorOffset = bytes[offset++];
                    cell.Light = bytes[offset++];

                    mapData.Cells[x, y] = cell;
                }
            }

            return mapData;
        }
        catch (Exception ex)
        {
            throw new InvalidDataException($"Error loading Type0 map: {ex.Message}", ex);
        }
    }

    // Placeholder methods for other map types - can be implemented later
    private static MapData LoadMapType1(byte[] bytes) => throw new NotImplementedException("Type1 maps not yet implemented");
    private static MapData LoadMapType2(byte[] bytes) => throw new NotImplementedException("Type2 maps not yet implemented");
    private static MapData LoadMapType3(byte[] bytes) => throw new NotImplementedException("Type3 maps not yet implemented");
    private static MapData LoadMapType4(byte[] bytes) => throw new NotImplementedException("Type4 maps not yet implemented");
    private static MapData LoadMapType5(byte[] bytes) => throw new NotImplementedException("Type5 maps not yet implemented");
    private static MapData LoadMapType6(byte[] bytes) => throw new NotImplementedException("Type6 maps not yet implemented");
    private static MapData LoadMapType7(byte[] bytes) => throw new NotImplementedException("Type7 maps not yet implemented");
}