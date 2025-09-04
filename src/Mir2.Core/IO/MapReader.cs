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
    /// Legacy reference: MapCode.cs LoadMapType0() for exact field sequence
    /// </summary>
    private static MapData LoadMapType0(byte[] bytes)
    {
        try
        {
            int offset = 0;
            int width = BitConverter.ToInt16(bytes, offset); // Legacy: MapCode.cs line 144
            offset += 2;
            int height = BitConverter.ToInt16(bytes, offset); // Legacy: MapCode.cs line 146
            
            var mapData = new MapData(width, height, MapFormatType.Type0);
            offset = 52; // Legacy: MapCode.cs line 148

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var cell = new CellInfo();
                    
                    // Legacy: MapCode.cs line 153-154 (sets defaults)
                    cell.BackIndex = 0;
                    cell.MiddleIndex = 1;
                    
                    cell.BackImage = BitConverter.ToInt16(bytes, offset); // Legacy: MapCode.cs line 155
                    offset += 2;
                    cell.MiddleImage = BitConverter.ToInt16(bytes, offset); // Legacy: MapCode.cs line 157
                    offset += 2;
                    cell.FrontImage = BitConverter.ToInt16(bytes, offset); // Legacy: MapCode.cs line 159
                    offset += 2;
                    cell.DoorIndex = bytes[offset++]; // Legacy: MapCode.cs line 161
                    cell.DoorOffset = bytes[offset++]; // Legacy: MapCode.cs line 162
                    cell.FrontAnimationFrame = bytes[offset++]; // Legacy: MapCode.cs line 163
                    cell.FrontAnimationTick = bytes[offset++]; // Legacy: MapCode.cs line 164
                    cell.FrontIndex = (short)(bytes[offset++] + 2); // Legacy: MapCode.cs line 165
                    cell.Light = bytes[offset++]; // Legacy: MapCode.cs line 166

                    // Legacy: MapCode.cs line 167-168 (bit masking)
                    if ((cell.BackImage & 0x8000) != 0)
                        cell.BackImage = (cell.BackImage & 0x7FFF) | 0x20000000;

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

    /// <summary>
    /// Loads Wemades 2010 map format (Type1)
    /// Legacy reference: MapCode.cs LoadMapType1()
    /// </summary>
    private static MapData LoadMapType1(byte[] bytes)
    {
        try
        {
            int offset = 21;

            int w = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            int xor = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            int h = BitConverter.ToInt16(bytes, offset);
            int width = w ^ xor;
            int height = h ^ xor;

            var mapData = new MapData(width, height, MapFormatType.Type1);
            offset = 54;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var cell = new CellInfo
                    {
                        BackIndex = 0, // Legacy: MapCode.cs line 203
                        BackImage = (int)(BitConverter.ToInt32(bytes, offset) ^ 0xAA38AA38), // Legacy: MapCode.cs line 204
                        MiddleIndex = 1, // Legacy: MapCode.cs line 205
                        MiddleImage = (short)(BitConverter.ToInt16(bytes, offset += 4) ^ xor), // Legacy: MapCode.cs line 206
                        FrontImage = (short)(BitConverter.ToInt16(bytes, offset += 2) ^ xor), // Legacy: MapCode.cs line 207
                        DoorIndex = bytes[offset += 2], // Legacy: MapCode.cs line 208
                        DoorOffset = bytes[++offset], // Legacy: MapCode.cs line 209
                        FrontAnimationFrame = bytes[++offset], // Legacy: MapCode.cs line 210
                        FrontAnimationTick = bytes[++offset], // Legacy: MapCode.cs line 211
                        FrontIndex = (short)(bytes[++offset] + 2), // Legacy: MapCode.cs line 212
                        Light = bytes[++offset], // Legacy: MapCode.cs line 213
                        Unknown = bytes[++offset] // Legacy: MapCode.cs line 214
                    };
                    offset++;

                    // Legacy: MapCode.cs line 218-221
                    if (cell.FrontIndex == 102)
                    {
                        cell.FrontIndex = 90;
                    }

                    mapData.Cells[x, y] = cell;
                }
            }

            return mapData;
        }
        catch (Exception ex)
        {
            throw new InvalidDataException($"Error loading Type1 map: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Loads Shanda 2012 format (Type2)  
    /// Legacy reference: MapCode.cs LoadMapType2()
    /// </summary>
    private static MapData LoadMapType2(byte[] bytes)
    {
        try
        {
            int offset = 0;
            int width = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            int height = BitConverter.ToInt16(bytes, offset);

            var mapData = new MapData(width, height, MapFormatType.Type2);
            offset = 52;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var cell = new CellInfo();
                    
                    cell.BackImage = BitConverter.ToInt16(bytes, offset); // Legacy: MapCode.cs line 247
                    offset += 2;
                    cell.MiddleImage = BitConverter.ToInt16(bytes, offset); // Legacy: MapCode.cs line 249
                    offset += 2;
                    cell.FrontImage = BitConverter.ToInt16(bytes, offset); // Legacy: MapCode.cs line 251
                    offset += 2;
                    cell.DoorIndex = bytes[offset++]; // Legacy: MapCode.cs line 253
                    cell.DoorOffset = bytes[offset++]; // Legacy: MapCode.cs line 254
                    cell.FrontAnimationFrame = bytes[offset++]; // Legacy: MapCode.cs line 255
                    cell.FrontAnimationTick = bytes[offset++]; // Legacy: MapCode.cs line 256
                    cell.FrontIndex = (short)(bytes[offset++] + 120); // Legacy: MapCode.cs line 257
                    cell.Light = bytes[offset++]; // Legacy: MapCode.cs line 258
                    cell.BackIndex = (short)(bytes[offset++] + 100); // Legacy: MapCode.cs line 259
                    cell.MiddleIndex = (short)(bytes[offset++] + 110); // Legacy: MapCode.cs line 260

                    // Legacy: MapCode.cs line 261-262
                    if ((cell.BackImage & 0x8000) != 0)
                        cell.BackImage = (cell.BackImage & 0x7FFF) | 0x20000000;

                    mapData.Cells[x, y] = cell;
                }
            }

            return mapData;
        }
        catch (Exception ex)
        {
            throw new InvalidDataException($"Error loading Type2 map: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Loads Extended Shanda format (Type3)
    /// Legacy reference: MapCode.cs LoadMapType3()
    /// </summary>
    private static MapData LoadMapType3(byte[] bytes)
    {
        try
        {
            int offset = 0;
            int width = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            int height = BitConverter.ToInt16(bytes, offset);

            var mapData = new MapData(width, height, MapFormatType.Type3);
            offset = 52;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var cell = new CellInfo();
                    
                    cell.BackImage = BitConverter.ToInt16(bytes, offset); // Legacy: MapCode.cs line 289
                    offset += 2;
                    cell.MiddleImage = BitConverter.ToInt16(bytes, offset); // Legacy: MapCode.cs line 291
                    offset += 2;
                    cell.FrontImage = BitConverter.ToInt16(bytes, offset); // Legacy: MapCode.cs line 293
                    offset += 2;
                    cell.DoorIndex = bytes[offset++]; // Legacy: MapCode.cs line 295
                    cell.DoorOffset = bytes[offset++]; // Legacy: MapCode.cs line 296
                    cell.FrontAnimationFrame = bytes[offset++]; // Legacy: MapCode.cs line 297
                    cell.FrontAnimationTick = bytes[offset++]; // Legacy: MapCode.cs line 298
                    cell.FrontIndex = (short)(bytes[offset++] + 120); // Legacy: MapCode.cs line 299
                    cell.Light = bytes[offset++]; // Legacy: MapCode.cs line 300
                    cell.BackIndex = (short)(bytes[offset++] + 100); // Legacy: MapCode.cs line 301
                    cell.MiddleIndex = (short)(bytes[offset++] + 110); // Legacy: MapCode.cs line 302
                    cell.TileAnimationImage = BitConverter.ToInt16(bytes, offset); // Legacy: MapCode.cs line 303
                    offset += 7; // Legacy: MapCode.cs line 304 (2bytes from tileanimframe, 2 bytes blank, 2bytes backtiles index, 1byte fileindex)
                    cell.TileAnimationFrames = bytes[offset++]; // Legacy: MapCode.cs line 305
                    cell.TileAnimationOffset = BitConverter.ToInt16(bytes, offset); // Legacy: MapCode.cs line 306
                    offset += 14; // Legacy: MapCode.cs line 307 (light, blending options)

                    // Legacy: MapCode.cs line 308-309
                    if ((cell.BackImage & 0x8000) != 0)
                        cell.BackImage = (cell.BackImage & 0x7FFF) | 0x20000000;

                    mapData.Cells[x, y] = cell;
                }
            }

            return mapData;
        }
        catch (Exception ex)
        {
            throw new InvalidDataException($"Error loading Type3 map: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Loads Wemades antihack map format (Type4)
    /// Legacy reference: MapCode.cs LoadMapType4()
    /// </summary>
    private static MapData LoadMapType4(byte[] bytes)
    {
        try
        {
            int offset = 31;
            int w = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            int xor = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            int h = BitConverter.ToInt16(bytes, offset);
            int width = w ^ xor;
            int height = h ^ xor;

            var mapData = new MapData(width, height, MapFormatType.Type4);
            offset = 64;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var cell = new CellInfo();
                    
                    cell.BackIndex = 0; // Legacy: MapCode.cs line 340
                    cell.MiddleIndex = 1; // Legacy: MapCode.cs line 341
                    cell.BackImage = (short)(BitConverter.ToInt16(bytes, offset) ^ xor); // Legacy: MapCode.cs line 342
                    offset += 2;
                    cell.MiddleImage = (short)(BitConverter.ToInt16(bytes, offset) ^ xor); // Legacy: MapCode.cs line 344
                    offset += 2;
                    cell.FrontImage = (short)(BitConverter.ToInt16(bytes, offset) ^ xor); // Legacy: MapCode.cs line 346
                    offset += 2;
                    cell.DoorIndex = bytes[offset++]; // Legacy: MapCode.cs line 348
                    cell.DoorOffset = bytes[offset++]; // Legacy: MapCode.cs line 349
                    cell.FrontAnimationFrame = bytes[offset++]; // Legacy: MapCode.cs line 350
                    cell.FrontAnimationTick = bytes[offset++]; // Legacy: MapCode.cs line 351
                    cell.FrontIndex = (short)(bytes[offset++] + 2); // Legacy: MapCode.cs line 352
                    cell.Light = bytes[offset++]; // Legacy: MapCode.cs line 353

                    // Legacy: MapCode.cs line 354-355
                    if ((cell.BackImage & 0x8000) != 0)
                        cell.BackImage = (cell.BackImage & 0x7FFF) | 0x20000000;

                    mapData.Cells[x, y] = cell;
                }
            }

            return mapData;
        }
        catch (Exception ex)
        {
            throw new InvalidDataException($"Error loading Type4 map: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Loads Wemade Mir3 format (Type5)
    /// Legacy reference: MapCode.cs LoadMapType5()
    /// </summary>
    private static MapData LoadMapType5(byte[] bytes)
    {
        try
        {
            byte flag = 0;
            int offset = 20;
            short attribute = BitConverter.ToInt16(bytes, offset); // Legacy: MapCode.cs line 373
            int width = BitConverter.ToInt16(bytes, offset += 2); // Legacy: MapCode.cs line 374
            int height = BitConverter.ToInt16(bytes, offset += 2); // Legacy: MapCode.cs line 375

            var mapData = new MapData(width, height, MapFormatType.Type5);
            offset = 28;

            // Initialize all cells - Legacy: MapCode.cs line 378-382
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    mapData.Cells[x, y] = new CellInfo();

            // Read all back tiles - Legacy: MapCode.cs line 383-393
            for (int x = 0; x < (width / 2); x++)
            {
                for (int y = 0; y < (height / 2); y++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        var cellX = (x * 2) + (i % 2);
                        var cellY = (y * 2) + (i / 2);
                        mapData.Cells[cellX, cellY].BackIndex = (short)(bytes[offset] != 255 ? bytes[offset] + 200 : -1); // Legacy: MapCode.cs line 389
                        mapData.Cells[cellX, cellY].BackImage = BitConverter.ToInt16(bytes, offset + 1) + 1; // Legacy: MapCode.cs line 390
                    }
                    offset += 3;
                }
            }

            // Read rest of data - Legacy: MapCode.cs line 394-424
            offset = 28 + (3 * ((width / 2) + (width % 2)) * (height / 2)); // Legacy: MapCode.cs line 395
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    flag = bytes[offset++]; // Legacy: MapCode.cs line 400
                    mapData.Cells[x, y].MiddleAnimationFrame = bytes[offset++]; // Legacy: MapCode.cs line 401
                    mapData.Cells[x, y].FrontAnimationFrame = bytes[offset] == 255 ? (byte)0 : bytes[offset]; // Legacy: MapCode.cs line 402
                    mapData.Cells[x, y].FrontAnimationFrame &= 0x8F; // Legacy: MapCode.cs line 403
                    offset++;
                    mapData.Cells[x, y].MiddleAnimationTick = 0; // Legacy: MapCode.cs line 405
                    mapData.Cells[x, y].FrontAnimationTick = 0; // Legacy: MapCode.cs line 406
                    mapData.Cells[x, y].FrontIndex = (short)(bytes[offset] != 255 ? bytes[offset] + 200 : -1); // Legacy: MapCode.cs line 407
                    offset++;
                    mapData.Cells[x, y].MiddleIndex = (short)(bytes[offset] != 255 ? bytes[offset] + 200 : -1); // Legacy: MapCode.cs line 409
                    offset++;
                    mapData.Cells[x, y].MiddleImage = (short)(BitConverter.ToInt16(bytes, offset) + 1); // Legacy: MapCode.cs line 411
                    offset += 2;
                    mapData.Cells[x, y].FrontImage = (short)(BitConverter.ToInt16(bytes, offset) + 1); // Legacy: MapCode.cs line 413
                    
                    // Legacy: MapCode.cs line 414-415
                    if ((mapData.Cells[x, y].FrontImage == 1) && (mapData.Cells[x, y].FrontIndex == 200))
                        mapData.Cells[x, y].FrontIndex = -1;
                    
                    offset += 2;
                    offset += 3; // Legacy: MapCode.cs line 417 (mir3 maps don't have doors)
                    mapData.Cells[x, y].Light = (byte)(bytes[offset] & 0x0F); // Legacy: MapCode.cs line 418
                    mapData.Cells[x, y].Light *= 4; // Legacy: MapCode.cs line 419 (far wants all light on mir3 maps maxed)
                    offset += 2;
                    
                    // Legacy: MapCode.cs line 421-422 (movement limit bits)
                    if ((flag & 0x01) != 1) mapData.Cells[x, y].BackImage |= 0x20000000;
                    if ((flag & 0x02) != 2) mapData.Cells[x, y].FrontImage = (short)((ushort)mapData.Cells[x, y].FrontImage | 0x8000);
                }
            }

            return mapData;
        }
        catch (Exception ex)
        {
            throw new InvalidDataException($"Error loading Type5 map: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Loads Shanda Mir3 format (Type6)
    /// Legacy reference: MapCode.cs LoadMapType6()
    /// </summary>
    private static MapData LoadMapType6(byte[] bytes)
    {
        try
        {
            byte flag = 0;
            int offset = 16;
            int width = BitConverter.ToInt16(bytes, offset); // Legacy: MapCode.cs line 438
            offset += 2;
            int height = BitConverter.ToInt16(bytes, offset); // Legacy: MapCode.cs line 440

            var mapData = new MapData(width, height, MapFormatType.Type6);
            offset = 40;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var cell = new CellInfo();
                    
                    flag = bytes[offset++]; // Legacy: MapCode.cs line 447
                    cell.BackIndex = (short)(bytes[offset] != 255 ? bytes[offset] + 300 : -1); // Legacy: MapCode.cs line 448
                    offset++;
                    cell.MiddleIndex = (short)(bytes[offset] != 255 ? bytes[offset] + 300 : -1); // Legacy: MapCode.cs line 450
                    offset++;
                    cell.FrontIndex = (short)(bytes[offset] != 255 ? bytes[offset] + 300 : -1); // Legacy: MapCode.cs line 452
                    offset++;
                    cell.BackImage = (short)(BitConverter.ToInt16(bytes, offset) + 1); // Legacy: MapCode.cs line 454
                    offset += 2;
                    cell.MiddleImage = (short)(BitConverter.ToInt16(bytes, offset) + 1); // Legacy: MapCode.cs line 456
                    offset += 2;
                    cell.FrontImage = (short)(BitConverter.ToInt16(bytes, offset) + 1); // Legacy: MapCode.cs line 458
                    offset += 2;
                    
                    // Legacy: MapCode.cs line 460-461
                    if ((cell.FrontImage == 1) && (cell.FrontIndex == 200))
                        cell.FrontIndex = -1;
                    
                    cell.MiddleAnimationFrame = bytes[offset++]; // Legacy: MapCode.cs line 462
                    cell.FrontAnimationFrame = bytes[offset] == 255 ? (byte)0 : bytes[offset]; // Legacy: MapCode.cs line 463
                    
                    // Legacy: MapCode.cs line 464-465
                    if (cell.FrontAnimationFrame > 0x0F)
                        cell.FrontAnimationFrame = (byte)(cell.FrontAnimationFrame & 0x0F);
                    
                    offset++;
                    cell.MiddleAnimationTick = 1; // Legacy: MapCode.cs line 467
                    cell.FrontAnimationTick = 1; // Legacy: MapCode.cs line 468
                    cell.Light = (byte)(bytes[offset] & 0x0F); // Legacy: MapCode.cs line 469
                    cell.Light *= 4; // Legacy: MapCode.cs line 470 (far wants all light on mir3 maps maxed)
                    offset += 8;
                    
                    // Legacy: MapCode.cs line 472-473 (movement limit bits)
                    if ((flag & 0x01) != 1) cell.BackImage |= 0x20000000;
                    if ((flag & 0x02) != 2) cell.FrontImage = (short)((ushort)cell.FrontImage | 0x8000);

                    mapData.Cells[x, y] = cell;
                }
            }

            return mapData;
        }
        catch (Exception ex)
        {
            throw new InvalidDataException($"Error loading Type6 map: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Loads 3/4 Heroes map format (Type7)
    /// Legacy reference: MapCode.cs LoadMapType7()
    /// </summary>
    private static MapData LoadMapType7(byte[] bytes)
    {
        try
        {
            int offset = 21;
            int width = BitConverter.ToInt16(bytes, offset); // Legacy: MapCode.cs line 489
            offset += 4;
            int height = BitConverter.ToInt16(bytes, offset); // Legacy: MapCode.cs line 491

            var mapData = new MapData(width, height, MapFormatType.Type7);
            offset = 54;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var cell = new CellInfo
                    {
                        BackIndex = 0, // Legacy: MapCode.cs line 501
                        BackImage = BitConverter.ToInt32(bytes, offset), // Legacy: MapCode.cs line 502
                        MiddleIndex = 1, // Legacy: MapCode.cs line 503
                        MiddleImage = BitConverter.ToInt16(bytes, offset += 4), // Legacy: MapCode.cs line 504
                        FrontImage = BitConverter.ToInt16(bytes, offset += 2), // Legacy: MapCode.cs line 505
                        DoorIndex = bytes[offset += 2], // Legacy: MapCode.cs line 506
                        DoorOffset = bytes[++offset], // Legacy: MapCode.cs line 507
                        FrontAnimationFrame = bytes[++offset], // Legacy: MapCode.cs line 508
                        FrontAnimationTick = bytes[++offset], // Legacy: MapCode.cs line 509
                        FrontIndex = (short)(bytes[++offset] + 2), // Legacy: MapCode.cs line 510
                        Light = bytes[++offset], // Legacy: MapCode.cs line 511
                        Unknown = bytes[++offset] // Legacy: MapCode.cs line 512
                    };
                    
                    // Legacy: MapCode.cs line 514-515
                    if ((cell.BackImage & 0x8000) != 0)
                        cell.BackImage = (cell.BackImage & 0x7FFF) | 0x20000000;
                    
                    offset++;

                    mapData.Cells[x, y] = cell;
                }
            }

            return mapData;
        }
        catch (Exception ex)
        {
            throw new InvalidDataException($"Error loading Type7 map: {ex.Message}", ex);
        }
    }
}