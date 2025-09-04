using Mir2.Core.Models;

namespace Mir2.Core.IO;

/// <summary>
/// Writes Mir2 map files in various formats (Types 0-7, 100) with legacy compatibility
/// </summary>
public class MapWriter
{
    /// <summary>
    /// Writes a map to the specified file path in the map's original format type
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
    /// Writes a map to a byte array in the map's original format type
    /// </summary>
    /// <param name="mapData">Map data to write</param>
    /// <returns>Byte array containing the map data</returns>
    public byte[] WriteToBytes(MapData mapData)
    {
        return mapData.FormatType switch
        {
            MapFormatType.Type100 => WriteType100(mapData),
            MapFormatType.Type0 => WriteType0(mapData),
            MapFormatType.Type1 => WriteType1(mapData),
            MapFormatType.Type2 => WriteType2(mapData),
            MapFormatType.Type3 => WriteType3(mapData),
            MapFormatType.Type4 => WriteType4(mapData),
            MapFormatType.Type5 => WriteType5(mapData),
            MapFormatType.Type6 => WriteType6(mapData),
            MapFormatType.Type7 => WriteType7(mapData),
            _ => WriteType100(mapData) // Default to Type100
        };
    }

    /// <summary>
    /// Writes Type100 (C# custom format)
    /// Legacy reference: MapCode.cs LoadMapType100() for field order
    /// </summary>
    private byte[] WriteType100(MapData mapData)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream);

        // Header: version (1 byte) + reserved (1 byte) + 'C#' tag (2 bytes)
        writer.Write((byte)1);   // Version 1 - Legacy: MapCode.cs line 533
        writer.Write((byte)0);   // Reserved
        writer.Write('C');       // C# tag part 1 - Legacy: MapCode.cs line 81
        writer.Write('#');       // C# tag part 2 - Legacy: MapCode.cs line 81

        // Write map dimensions - Legacy: MapCode.cs line 534-536
        writer.Write((short)mapData.Width);
        writer.Write((short)mapData.Height);

        // Write cell data in exact legacy order
        for (int x = 0; x < mapData.Width; x++)
        {
            for (int y = 0; y < mapData.Height; y++)
            {
                var cell = mapData.Cells[x, y] ?? new CellInfo();
                
                writer.Write(cell.BackIndex);          // Legacy: MapCode.cs line 543
                writer.Write(cell.BackImage);          // Legacy: MapCode.cs line 551 (4 bytes)
                writer.Write(cell.MiddleIndex);        // Legacy: MapCode.cs line 555
                writer.Write(cell.MiddleImage);        // Legacy: MapCode.cs line 557
                writer.Write(cell.FrontIndex);         // Legacy: MapCode.cs line 559
                writer.Write(cell.FrontImage);         // Legacy: MapCode.cs line 561
                writer.Write(cell.DoorIndex);          // Legacy: MapCode.cs line 563
                writer.Write(cell.DoorOffset);         // Legacy: MapCode.cs line 564
                writer.Write(cell.FrontAnimationFrame); // Legacy: MapCode.cs line 565
                writer.Write(cell.FrontAnimationTick);  // Legacy: MapCode.cs line 566
                writer.Write(cell.MiddleAnimationFrame); // Legacy: MapCode.cs line 567
                writer.Write(cell.MiddleAnimationTick);  // Legacy: MapCode.cs line 568
                writer.Write(cell.TileAnimationImage);   // Legacy: MapCode.cs line 569
                writer.Write(cell.TileAnimationOffset);  // Legacy: MapCode.cs line 571
                writer.Write(cell.TileAnimationFrames);  // Legacy: MapCode.cs line 573
                writer.Write(cell.Light);               // Legacy: MapCode.cs line 574
            }
        }

        writer.Flush();
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Writes Type0 (Default old school format)
    /// Legacy reference: MapCode.cs LoadMapType0() for field order - 12 bytes per cell
    /// </summary>
    private byte[] WriteType0(MapData mapData)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream);

        // Write dimensions - Legacy: MapCode.cs line 144-146
        writer.Write((short)mapData.Width);
        writer.Write((short)mapData.Height);
        
        // Write 48 bytes of padding to reach offset 52
        writer.Write(new byte[48]);

        // Write cell data - Legacy: MapCode.cs line 155-166 (12 bytes per cell)
        for (int x = 0; x < mapData.Width; x++)
        {
            for (int y = 0; y < mapData.Height; y++)
            {
                var cell = mapData.Cells[x, y] ?? new CellInfo();
                
                // Convert back to legacy format (remove modifications)
                var backImage = cell.BackImage;
                if ((backImage & 0x20000000) != 0)
                    backImage = (backImage & ~0x20000000) | 0x8000;

                writer.Write((short)backImage);         // Legacy: MapCode.cs line 155
                writer.Write(cell.MiddleImage);         // Legacy: MapCode.cs line 157
                writer.Write(cell.FrontImage);          // Legacy: MapCode.cs line 159
                writer.Write(cell.DoorIndex);           // Legacy: MapCode.cs line 161
                writer.Write(cell.DoorOffset);          // Legacy: MapCode.cs line 162
                writer.Write(cell.FrontAnimationFrame); // Legacy: MapCode.cs line 163
                writer.Write(cell.FrontAnimationTick);  // Legacy: MapCode.cs line 164
                writer.Write((byte)Math.Max(0, cell.FrontIndex - 2)); // Legacy: MapCode.cs line 165
                writer.Write(cell.Light);               // Legacy: MapCode.cs line 166
                // Total: 12 bytes per cell
            }
        }

        writer.Flush();
        return memoryStream.ToArray();
    }

    // For brevity, implementing Type1 and Type4 which require XOR encryption
    /// <summary>
    /// Writes Type1 (Wemades 2010 map format)
    /// Legacy reference: MapCode.cs LoadMapType1() for field order and XOR
    /// </summary>
    private byte[] WriteType1(MapData mapData)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream);

        // Use fixed XOR key for debugging - will use random later
        var xor = (short)0x1234; // Fixed for debugging

        // Write header - Legacy: "Map 2010 Ver 1.0" - Detection bytes[0] == 0x10 && bytes[2] == 0x61 && bytes[7] == 0x31 && bytes[14] == 0x31
        writer.Write((byte)0x10);        // bytes[0]
        writer.Write((byte)'M');         // bytes[1]
        writer.Write((byte)0x61);        // bytes[2] 'a'
        writer.Write((byte)'p');         // bytes[3]
        writer.Write((byte)' ');         // bytes[4]
        writer.Write((byte)'2');         // bytes[5]
        writer.Write((byte)'0');         // bytes[6]
        writer.Write((byte)0x31);        // bytes[7] '1'
        writer.Write((byte)'0');         // bytes[8]
        writer.Write((byte)' ');         // bytes[9]
        writer.Write((byte)'V');         // bytes[10]
        writer.Write((byte)'e');         // bytes[11]
        writer.Write((byte)'r');         // bytes[12]
        writer.Write((byte)' ');         // bytes[13]
        writer.Write((byte)0x31);        // bytes[14] '1'
        writer.Write((byte)'.');         // bytes[15]
        writer.Write((byte)'0');         // bytes[16]
        writer.Write(new byte[4]);       // Padding to offset 21

        writer.Write((short)(mapData.Width ^ xor));  // Legacy: MapCode.cs line 187
        writer.Write(xor);                           // Legacy: MapCode.cs line 189
        writer.Write((short)(mapData.Height ^ xor)); // Legacy: MapCode.cs line 191

        writer.Write(new byte[29]); // Padding to offset 54

        // Write cell data with XOR encryption
        for (int x = 0; x < mapData.Width; x++)
        {
            for (int y = 0; y < mapData.Height; y++)
            {
                var cell = mapData.Cells[x, y] ?? new CellInfo();
                
                uint xorResult = (uint)cell.BackImage ^ 0xAA38AA38u;
                writer.Write((int)xorResult); // Explicit int write - Legacy: MapCode.cs line 204
                writer.Write((short)(cell.MiddleImage ^ xor));     // Legacy: MapCode.cs line 206
                writer.Write((short)(cell.FrontImage ^ xor));      // Legacy: MapCode.cs line 207
                writer.Write(cell.DoorIndex);                      // Legacy: MapCode.cs line 208
                writer.Write(cell.DoorOffset);                     // Legacy: MapCode.cs line 209
                writer.Write(cell.FrontAnimationFrame);            // Legacy: MapCode.cs line 210
                writer.Write(cell.FrontAnimationTick);             // Legacy: MapCode.cs line 211
                
                // Handle FrontIndex conversion - Legacy: MapCode.cs line 212, 218-221
                var frontIndex = cell.FrontIndex;
                if (frontIndex == 90) frontIndex = 102;
                writer.Write((byte)Math.Max(0, frontIndex - 2));
                
                writer.Write(cell.Light);                          // Legacy: MapCode.cs line 213
                writer.Write(cell.Unknown);                        // Legacy: MapCode.cs line 214
                writer.Write((byte)0);                            // Padding
            }
        }

        writer.Flush();
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Writes Type4 (Wemades antihack map format)
    /// Legacy reference: MapCode.cs LoadMapType4() for field order and XOR
    /// </summary>
    private byte[] WriteType4(MapData mapData)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream);

        // Generate XOR key
        var random = new Random();
        var xor = (short)random.Next(1, 0x7FFF);

        // Write header - Legacy: "Mir2 AntiHack" - Detection bytes[0] == 0x15 && bytes[4] == 0x32 && bytes[6] == 0x41 && bytes[19] == 0x31
        writer.Write((byte)0x15);        // bytes[0]
        writer.Write((byte)'M');         // bytes[1]
        writer.Write((byte)'i');         // bytes[2]
        writer.Write((byte)'r');         // bytes[3]
        writer.Write((byte)0x32);        // bytes[4] '2'
        writer.Write((byte)' ');         // bytes[5]
        writer.Write((byte)0x41);        // bytes[6] 'A'
        writer.Write((byte)'n');         // bytes[7]
        writer.Write((byte)'t');         // bytes[8]
        writer.Write((byte)'i');         // bytes[9]
        writer.Write((byte)'H');         // bytes[10]
        writer.Write((byte)'a');         // bytes[11]
        writer.Write((byte)'c');         // bytes[12]
        writer.Write((byte)'k');         // bytes[13]
        writer.Write(new byte[17]);      // Padding  
        writer.Write((byte)0x31);        // bytes[19] - This seems to be part of the detection
        writer.Write(new byte[11]);      // More padding to offset 31

        writer.Write((short)(mapData.Width ^ xor));  // Legacy: MapCode.cs line 327
        writer.Write(xor);                           // Legacy: MapCode.cs line 329
        writer.Write((short)(mapData.Height ^ xor)); // Legacy: MapCode.cs line 331

        writer.Write(new byte[29]); // Padding to offset 64

        // Write cell data with XOR encryption
        for (int x = 0; x < mapData.Width; x++)
        {
            for (int y = 0; y < mapData.Height; y++)
            {
                var cell = mapData.Cells[x, y] ?? new CellInfo();
                
                // Convert back to legacy format
                var backImage = (short)cell.BackImage;
                if ((cell.BackImage & 0x20000000) != 0)
                    backImage = (short)((cell.BackImage & ~0x20000000) | 0x8000);

                writer.Write((short)(backImage ^ xor));            // Legacy: MapCode.cs line 342
                writer.Write((short)(cell.MiddleImage ^ xor));      // Legacy: MapCode.cs line 344
                writer.Write((short)(cell.FrontImage ^ xor));      // Legacy: MapCode.cs line 346
                writer.Write(cell.DoorIndex);                      // Legacy: MapCode.cs line 348
                writer.Write(cell.DoorOffset);                     // Legacy: MapCode.cs line 349
                writer.Write(cell.FrontAnimationFrame);            // Legacy: MapCode.cs line 350
                writer.Write(cell.FrontAnimationTick);             // Legacy: MapCode.cs line 351
                writer.Write((byte)Math.Max(0, cell.FrontIndex - 2)); // Legacy: MapCode.cs line 352
                writer.Write(cell.Light);                          // Legacy: MapCode.cs line 353
            }
        }

        writer.Flush();
        return memoryStream.ToArray();
    }

    // Placeholders for other types - implementing the most complex ones for now
    private byte[] WriteType2(MapData mapData) => throw new NotImplementedException("Type2 writing not yet implemented");
    private byte[] WriteType3(MapData mapData) => throw new NotImplementedException("Type3 writing not yet implemented");
    private byte[] WriteType5(MapData mapData) => throw new NotImplementedException("Type5 writing not yet implemented");
    private byte[] WriteType6(MapData mapData) => throw new NotImplementedException("Type6 writing not yet implemented");
    private byte[] WriteType7(MapData mapData) => throw new NotImplementedException("Type7 writing not yet implemented");

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