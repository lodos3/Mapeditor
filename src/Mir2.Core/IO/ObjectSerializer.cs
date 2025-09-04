using Mir2.Core.Models;

namespace Mir2.Core.IO;

/// <summary>
/// Serializes and deserializes .X object files with exact legacy compatibility
/// Legacy reference: Main.cs SaveObjectsFile() and ReadObjectsFile() methods
/// </summary>
public class ObjectSerializer
{
    /// <summary>
    /// Saves object data to a .X file
    /// Legacy reference: Main.cs SaveObjectsFile() method
    /// </summary>
    /// <param name="objectData">Object data to save</param>
    /// <param name="filePath">Path to save the .X file</param>
    public async Task SaveAsync(CellInfoData[] objectData, string filePath)
    {
        var bytes = SerializeToBytes(objectData);
        await File.WriteAllBytesAsync(filePath, bytes);
    }

    /// <summary>
    /// Loads object data from a .X file
    /// Legacy reference: Main.cs ReadObjectsFile() method
    /// </summary>
    /// <param name="filePath">Path to the .X file</param>
    /// <returns>Array of cell info data, or empty array if file doesn't exist</returns>
    public async Task<CellInfoData[]> LoadAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return Array.Empty<CellInfoData>();

        var bytes = await File.ReadAllBytesAsync(filePath);
        return DeserializeFromBytes(bytes);
    }

    /// <summary>
    /// Serializes object data to byte array with exact legacy format
    /// </summary>
    /// <param name="objectData">Object data to serialize</param>
    /// <returns>Byte array in .X format</returns>
    public byte[] SerializeToBytes(CellInfoData[] objectData)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream);

        // Write count - Legacy: Main.cs line 2154
        writer.Write(objectData.Length);

        // Write each object - Legacy: Main.cs lines 2155-2175
        for (int i = 0; i < objectData.Length; i++)
        {
            var data = objectData[i];
            var cell = data.CellInfo;

            writer.Write(data.X);                        // Legacy: Main.cs line 2157
            writer.Write(data.Y);                        // Legacy: Main.cs line 2158
            writer.Write(cell.BackIndex);                // Legacy: Main.cs line 2159
            writer.Write(cell.BackImage);                // Legacy: Main.cs line 2160
            writer.Write(cell.MiddleIndex);              // Legacy: Main.cs line 2161
            writer.Write(cell.MiddleImage);              // Legacy: Main.cs line 2162
            writer.Write(cell.FrontIndex);               // Legacy: Main.cs line 2163
            writer.Write(cell.FrontImage);               // Legacy: Main.cs line 2164
            writer.Write(cell.DoorIndex);                // Legacy: Main.cs line 2165
            writer.Write(cell.DoorOffset);               // Legacy: Main.cs line 2166
            writer.Write(cell.FrontAnimationFrame);      // Legacy: Main.cs line 2167
            writer.Write(cell.FrontAnimationTick);       // Legacy: Main.cs line 2168
            writer.Write(cell.MiddleAnimationFrame);     // Legacy: Main.cs line 2169
            writer.Write(cell.MiddleAnimationTick);      // Legacy: Main.cs line 2170
            writer.Write(cell.TileAnimationImage);       // Legacy: Main.cs line 2171
            writer.Write(cell.TileAnimationOffset);      // Legacy: Main.cs line 2172
            writer.Write(cell.TileAnimationFrames);      // Legacy: Main.cs line 2173
            writer.Write(cell.Light);                    // Legacy: Main.cs line 2174
        }

        writer.Flush();
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Deserializes object data from byte array with exact legacy format
    /// </summary>
    /// <param name="bytes">Byte array in .X format</param>
    /// <returns>Array of cell info data</returns>
    public CellInfoData[] DeserializeFromBytes(byte[] bytes)
    {
        if (bytes.Length == 0)
            return Array.Empty<CellInfoData>();

        using var memoryStream = new MemoryStream(bytes);
        using var reader = new BinaryReader(memoryStream);

        try
        {
            int offset = 0;
            
            // Read count - Legacy: Main.cs line 2311
            int count = BitConverter.ToInt32(bytes, offset);
            offset += 4;

            if (count <= 0)
                return Array.Empty<CellInfoData>();

            var objectDatas = new CellInfoData[count];

            // Read each object - Legacy: Main.cs lines 2314-2349
            for (int i = 0; i < count; i++)
            {
                objectDatas[i] = new CellInfoData();
                objectDatas[i].CellInfo = new CellInfo();

                objectDatas[i].X = BitConverter.ToInt32(bytes, offset);                                    // Legacy: Main.cs line 2318
                offset += 4;
                objectDatas[i].Y = BitConverter.ToInt32(bytes, offset);                                    // Legacy: Main.cs line 2320
                offset += 4;
                objectDatas[i].CellInfo.BackIndex = BitConverter.ToInt16(bytes, offset);                  // Legacy: Main.cs line 2322
                offset += 2;
                objectDatas[i].CellInfo.BackImage = BitConverter.ToInt32(bytes, offset);                  // Legacy: Main.cs line 2324
                offset += 4;
                objectDatas[i].CellInfo.MiddleIndex = BitConverter.ToInt16(bytes, offset);                // Legacy: Main.cs line 2326
                offset += 2;
                objectDatas[i].CellInfo.MiddleImage = BitConverter.ToInt16(bytes, offset);                // Legacy: Main.cs line 2328
                offset += 2;
                objectDatas[i].CellInfo.FrontIndex = BitConverter.ToInt16(bytes, offset);                 // Legacy: Main.cs line 2330
                offset += 2;
                objectDatas[i].CellInfo.FrontImage = BitConverter.ToInt16(bytes, offset);                 // Legacy: Main.cs line 2332
                offset += 2;
                objectDatas[i].CellInfo.DoorIndex = bytes[offset++];                                       // Legacy: Main.cs line 2334
                objectDatas[i].CellInfo.DoorOffset = bytes[offset++];                                      // Legacy: Main.cs line 2335
                objectDatas[i].CellInfo.FrontAnimationFrame = bytes[offset++];                            // Legacy: Main.cs line 2336
                objectDatas[i].CellInfo.FrontAnimationTick = bytes[offset++];                             // Legacy: Main.cs line 2337
                objectDatas[i].CellInfo.MiddleAnimationFrame = bytes[offset++];                           // Legacy: Main.cs line 2338
                objectDatas[i].CellInfo.MiddleAnimationTick = bytes[offset++];                            // Legacy: Main.cs line 2339
                objectDatas[i].CellInfo.TileAnimationImage = BitConverter.ToInt16(bytes, offset);         // Legacy: Main.cs line 2340
                offset += 2;
                objectDatas[i].CellInfo.TileAnimationOffset = BitConverter.ToInt16(bytes, offset);        // Legacy: Main.cs line 2342
                offset += 2;
                objectDatas[i].CellInfo.TileAnimationFrames = bytes[offset++];                            // Legacy: Main.cs line 2344
                objectDatas[i].CellInfo.Light = bytes[offset++];                                           // Legacy: Main.cs line 2345

                // Note: FishingCell is computed automatically from Light range 100-119
            }

            return objectDatas;
        }
        catch (Exception ex)
        {
            throw new InvalidDataException($"Error deserializing .X object file: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Synchronous version of SaveAsync
    /// </summary>
    public void Save(CellInfoData[] objectData, string filePath)
    {
        SaveAsync(objectData, filePath).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Synchronous version of LoadAsync
    /// </summary>
    public CellInfoData[] Load(string filePath)
    {
        return LoadAsync(filePath).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Calculates the expected file size for object data
    /// </summary>
    /// <param name="objectCount">Number of objects</param>
    /// <returns>Expected file size in bytes</returns>
    public static long CalculateFileSize(int objectCount)
    {
        // 4 bytes for count + (objectCount * bytes per object)
        // Each object: 4(X) + 4(Y) + 2(BackIndex) + 4(BackImage) + 2(MiddleIndex) + 2(MiddleImage) + 2(FrontIndex) + 2(FrontImage) + 1(DoorIndex) + 1(DoorOffset) + 1(FrontAnimationFrame) + 1(FrontAnimationTick) + 1(MiddleAnimationFrame) + 1(MiddleAnimationTick) + 2(TileAnimationImage) + 2(TileAnimationOffset) + 1(TileAnimationFrames) + 1(Light) = 34 bytes
        return 4 + (objectCount * 34);
    }
}