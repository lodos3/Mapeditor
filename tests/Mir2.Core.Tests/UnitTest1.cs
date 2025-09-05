using Mir2.Core.IO;
using Mir2.Core.Models;
using Mir2.Core.Services;

namespace Mir2.Core.Tests;

public class MapReaderWriterTests
{
    [Fact]
    public void MapData_DefaultConstructor_CreatesValidMap()
    {
        // Arrange & Act
        var map = new MapData();

        // Assert
        Assert.Equal(1000, map.Width);
        Assert.Equal(1000, map.Height);
        Assert.Equal(MapFormatType.Type100, map.FormatType);
        Assert.NotNull(map.Cells);
        Assert.Equal(1000, map.Cells.GetLength(0));
        Assert.Equal(1000, map.Cells.GetLength(1));

        // Check that all cells are initialized
        for (int x = 0; x < 10; x++) // Just check a few cells
        {
            for (int y = 0; y < 10; y++)
            {
                Assert.NotNull(map.Cells[x, y]);
            }
        }
    }

    [Fact]
    public void MapData_CustomConstructor_CreatesValidMap()
    {
        // Arrange & Act
        var map = new MapData(100, 200, MapFormatType.Type0);

        // Assert
        Assert.Equal(100, map.Width);
        Assert.Equal(200, map.Height);
        Assert.Equal(MapFormatType.Type0, map.FormatType);
        Assert.NotNull(map.Cells);
        Assert.Equal(100, map.Cells.GetLength(0));
        Assert.Equal(200, map.Cells.GetLength(1));
    }

    [Fact]
    public void CellInfo_FishingCell_DerivedFromLight()
    {
        // Arrange
        var cellNormal = new CellInfo { Light = 50 };
        var cellFishing100 = new CellInfo { Light = 100 };
        var cellFishing101 = new CellInfo { Light = 101 };
        var cellFishing119 = new CellInfo { Light = 119 };
        var cellAboveRange = new CellInfo { Light = 120 };
        var cellBelowRange = new CellInfo { Light = 99 };

        // Act & Assert
        Assert.False(cellNormal.FishingCell);
        Assert.False(cellBelowRange.FishingCell);
        Assert.False(cellAboveRange.FishingCell);
        Assert.True(cellFishing100.FishingCell);
        Assert.True(cellFishing101.FishingCell);
        Assert.True(cellFishing119.FishingCell);
    }

    [Fact]
    public void CellInfo_Clone_CreatesDeepCopy()
    {
        // Arrange
        var original = new CellInfo
        {
            BackIndex = 1,
            BackImage = 2,
            MiddleIndex = 3,
            MiddleImage = 4,
            FrontIndex = 5,
            FrontImage = 6,
            Light = 100,
            DoorIndex = 7
        };

        // Act
        var clone = original.Clone();

        // Assert
        Assert.NotSame(original, clone);
        Assert.Equal(original.BackIndex, clone.BackIndex);
        Assert.Equal(original.BackImage, clone.BackImage);
        Assert.Equal(original.MiddleIndex, clone.MiddleIndex);
        Assert.Equal(original.MiddleImage, clone.MiddleImage);
        Assert.Equal(original.FrontIndex, clone.FrontIndex);
        Assert.Equal(original.FrontImage, clone.FrontImage);
        Assert.Equal(original.Light, clone.Light);
        Assert.Equal(original.DoorIndex, clone.DoorIndex);
        Assert.Equal(original.FishingCell, clone.FishingCell);
    }

    [Fact]
    public void MapReader_ReadFromBytes_EmptyBytes_ReturnsDefaultMap()
    {
        // Arrange
        var reader = new MapReader();
        var emptyBytes = Array.Empty<byte>();

        // Act
        var map = reader.ReadFromBytes(emptyBytes);

        // Assert
        Assert.NotNull(map);
        Assert.Equal(1000, map.Width);
        Assert.Equal(1000, map.Height);
    }

    [Fact]
    public void MapWriter_WriteToBytes_ValidMap_ProducesValidBytes()
    {
        // Arrange
        var writer = new MapWriter();
        var map = new MapData(10, 10);
        
        // Set some test data
        map.Cells[0, 0].BackIndex = 1;
        map.Cells[0, 0].BackImage = 2;
        map.Cells[0, 0].Light = 50;

        // Act
        var bytes = writer.WriteToBytes(map);

        // Assert
        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);
        
        // Check header: version (2) + 'C#' (2) + dimensions (4) = 8 bytes
        // Plus cell data: 10 * 10 * 26 = 2600 bytes
        // Total: 2608 bytes
        Assert.Equal(2608, bytes.Length);

        // Check header
        Assert.Equal(1, BitConverter.ToInt16(bytes, 0)); // Version
        Assert.Equal('C', (char)bytes[2]); // C# tag part 1
        Assert.Equal('#', (char)bytes[3]); // C# tag part 2
        Assert.Equal(10, BitConverter.ToInt16(bytes, 4)); // Width
        Assert.Equal(10, BitConverter.ToInt16(bytes, 6)); // Height
    }

    [Fact]
    public void MapReaderWriter_RoundTrip_Type100_PreservesData()
    {
        // Arrange
        var originalMap = new MapData(5, 5);
        
        // Set some test data with various values
        originalMap.Cells[0, 0] = new CellInfo
        {
            BackIndex = 1,
            BackImage = 12345,
            MiddleIndex = 2,
            MiddleImage = 67,
            FrontIndex = 3,
            FrontImage = 89,
            DoorIndex = 4,
            DoorOffset = 5,
            FrontAnimationFrame = 6,
            FrontAnimationTick = 7,
            MiddleAnimationFrame = 8,
            MiddleAnimationTick = 9,
            TileAnimationImage = 10,
            TileAnimationOffset = 11,
            TileAnimationFrames = 12,
            Light = 100 // Fishing cell
        };

        originalMap.Cells[4, 4] = new CellInfo
        {
            BackIndex = -1,
            BackImage = -12345,
            Light = 50
        };

        var writer = new MapWriter();
        var reader = new MapReader();

        // Act
        var bytes = writer.WriteToBytes(originalMap);
        var loadedMap = reader.ReadFromBytes(bytes);

        // Assert
        Assert.Equal(originalMap.Width, loadedMap.Width);
        Assert.Equal(originalMap.Height, loadedMap.Height);
        Assert.Equal(MapFormatType.Type100, loadedMap.FormatType);

        // Check specific cell data
        var originalCell = originalMap.Cells[0, 0];
        var loadedCell = loadedMap.Cells[0, 0];

        Assert.Equal(originalCell.BackIndex, loadedCell.BackIndex);
        Assert.Equal(originalCell.BackImage, loadedCell.BackImage);
        Assert.Equal(originalCell.MiddleIndex, loadedCell.MiddleIndex);
        Assert.Equal(originalCell.MiddleImage, loadedCell.MiddleImage);
        Assert.Equal(originalCell.FrontIndex, loadedCell.FrontIndex);
        Assert.Equal(originalCell.FrontImage, loadedCell.FrontImage);
        Assert.Equal(originalCell.DoorIndex, loadedCell.DoorIndex);
        Assert.Equal(originalCell.DoorOffset, loadedCell.DoorOffset);
        Assert.Equal(originalCell.FrontAnimationFrame, loadedCell.FrontAnimationFrame);
        Assert.Equal(originalCell.FrontAnimationTick, loadedCell.FrontAnimationTick);
        Assert.Equal(originalCell.MiddleAnimationFrame, loadedCell.MiddleAnimationFrame);
        Assert.Equal(originalCell.MiddleAnimationTick, loadedCell.MiddleAnimationTick);
        Assert.Equal(originalCell.TileAnimationImage, loadedCell.TileAnimationImage);
        Assert.Equal(originalCell.TileAnimationOffset, loadedCell.TileAnimationOffset);
        Assert.Equal(originalCell.TileAnimationFrames, loadedCell.TileAnimationFrames);
        Assert.Equal(originalCell.Light, loadedCell.Light);
        Assert.Equal(originalCell.FishingCell, loadedCell.FishingCell); // Should be true for light = 100

        // Check another cell
        var originalCell2 = originalMap.Cells[4, 4];
        var loadedCell2 = loadedMap.Cells[4, 4];

        Assert.Equal(originalCell2.BackIndex, loadedCell2.BackIndex);
        Assert.Equal(originalCell2.BackImage, loadedCell2.BackImage);
        Assert.Equal(originalCell2.Light, loadedCell2.Light);
        Assert.Equal(originalCell2.FishingCell, loadedCell2.FishingCell); // Should be false for light = 50
    }

    [Fact]
    public void MapWriter_ValidateMapData_ValidMap_ReturnsTrue()
    {
        // Arrange
        var validMap = new MapData(100, 100);

        // Act & Assert
        Assert.True(MapWriter.ValidateMapData(validMap));
    }

    [Fact]
    public void MapWriter_ValidateMapData_InvalidDimensionsInData_ReturnsFalse()
    {
        // Arrange
        var mapWithInvalidDimensions = new MapData(100, 100);
        // Manually set invalid dimensions to test validation logic
        mapWithInvalidDimensions.Width = 0;

        // Act & Assert
        Assert.False(MapWriter.ValidateMapData(mapWithInvalidDimensions));
    }

    [Theory]
    [InlineData(0, 100)] // Invalid width
    [InlineData(100, 0)] // Invalid height
    [InlineData(-1, 100)] // Negative width
    [InlineData(100, -1)] // Negative height
    [InlineData(20000, 100)] // Too large width
    [InlineData(100, 20000)] // Too large height
    public void MapData_Constructor_InvalidDimensions_ThrowsException(int width, int height)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new MapData(width, height));
    }

    [Fact]
    public void MapWriter_ValidateMapData_NullMap_ReturnsFalse()
    {
        // Act & Assert
        Assert.False(MapWriter.ValidateMapData(null!));
    }

    [Fact]
    public void MapWriter_CalculateFileSize_ReturnsCorrectSize()
    {
        // Arrange
        int width = 100;
        int height = 200;

        // Act
        var size = MapWriter.CalculateFileSize(width, height);

        // Assert
        // Header (8 bytes) + cells (100 * 200 * 26 bytes per cell)
        var expected = 8 + (100 * 200 * 26);
        Assert.Equal(expected, size);
    }

    [Fact]
    public void MapReaderWriter_RoundTrip_Type0_PreservesData()
    {
        // Arrange
        var originalMap = new MapData(5, 5, MapFormatType.Type0);
        originalMap.Cells[2, 2] = new CellInfo
        {
            BackImage = 100,
            MiddleImage = 200,
            FrontImage = 300,
            FrontIndex = 5,
            DoorIndex = 1,
            DoorOffset = 2,
            FrontAnimationFrame = 3,
            FrontAnimationTick = 4,
            Light = 110
        };

        // Act
        var writer = new MapWriter();
        var bytes = writer.WriteToBytes(originalMap);
        
        var reader = new MapReader();
        var loadedMap = reader.ReadFromBytes(bytes);

        // Assert
        var originalCell = originalMap.Cells[2, 2];
        var loadedCell = loadedMap.Cells[2, 2];

        Assert.Equal(originalCell.BackImage, loadedCell.BackImage);
        Assert.Equal(originalCell.MiddleImage, loadedCell.MiddleImage);
        Assert.Equal(originalCell.FrontImage, loadedCell.FrontImage);
        Assert.Equal(originalCell.FrontIndex, loadedCell.FrontIndex);
        Assert.Equal(originalCell.Light, loadedCell.Light);
        Assert.Equal(originalCell.FishingCell, loadedCell.FishingCell);
    }

    [Fact]
    public void MapReaderWriter_RoundTrip_Type1_PreservesData()
    {
        // Arrange
        var originalMap = new MapData(3, 3, MapFormatType.Type1);
        originalMap.Cells[1, 1] = new CellInfo
        {
            BackImage = 123456,
            MiddleImage = 789,
            FrontImage = 321,
            FrontIndex = 90, // Should be converted to 102 and back
            Light = 105,
            Unknown = 42
        };

        // Act
        var writer = new MapWriter();
        var bytes = writer.WriteToBytes(originalMap);
        
        var reader = new MapReader();
        var loadedMap = reader.ReadFromBytes(bytes);

        // Assert
        var originalCell = originalMap.Cells[1, 1];
        var loadedCell = loadedMap.Cells[1, 1];

        Assert.Equal(originalCell.BackImage, loadedCell.BackImage);
        Assert.Equal(originalCell.MiddleImage, loadedCell.MiddleImage);
        Assert.Equal(originalCell.FrontImage, loadedCell.FrontImage);
        Assert.Equal(originalCell.FrontIndex, loadedCell.FrontIndex);
        Assert.Equal(originalCell.Light, loadedCell.Light);
        Assert.Equal(originalCell.Unknown, loadedCell.Unknown);
        Assert.Equal(originalCell.FishingCell, loadedCell.FishingCell);
    }

    [Fact]
    public void MapReaderWriter_RoundTrip_Type4_PreservesData()
    {
        // Arrange  
        var originalMap = new MapData(4, 4, MapFormatType.Type4);
        originalMap.Cells[2, 1] = new CellInfo
        {
            BackImage = 0x18000, // Test bit masking
            MiddleImage = 456,
            FrontImage = 789,
            FrontIndex = 7,
            Light = 115
        };

        // Act
        var writer = new MapWriter();
        var bytes = writer.WriteToBytes(originalMap);
        
        var reader = new MapReader();
        var loadedMap = reader.ReadFromBytes(bytes);

        // Assert
        var originalCell = originalMap.Cells[2, 1];
        var loadedCell = loadedMap.Cells[2, 1];

        // BackImage should be preserved with bit transformation
        Assert.Equal(originalCell.BackImage, loadedCell.BackImage);
        Assert.Equal(originalCell.MiddleImage, loadedCell.MiddleImage);
        Assert.Equal(originalCell.FrontImage, loadedCell.FrontImage);
        Assert.Equal(originalCell.FrontIndex, loadedCell.FrontIndex);
        Assert.Equal(originalCell.Light, loadedCell.Light);
        Assert.Equal(originalCell.FishingCell, loadedCell.FishingCell);
    }

    [Fact]
    public void Debug_Type0_WriteRead()
    {
        // Arrange - very simple map to debug
        var originalMap = new MapData(2, 2, MapFormatType.Type0);
        originalMap.Cells[0, 0] = new CellInfo { Light = 50 };
        originalMap.Cells[1, 1] = new CellInfo { Light = 100 };

        // Act
        var writer = new MapWriter();
        var bytes = writer.WriteToBytes(originalMap);
        
        // Debug - check the file size and initial bytes
        System.Console.WriteLine($"Generated {bytes.Length} bytes for 2x2 Type0 map");
        System.Console.WriteLine($"First 10 bytes: {string.Join(", ", bytes.Take(10).Select(b => $"0x{b:X2}"))}");
        
        var reader = new MapReader();
        var loadedMap = reader.ReadFromBytes(bytes);

        // Assert
        Assert.Equal(originalMap.Width, loadedMap.Width);
        Assert.Equal(originalMap.Height, loadedMap.Height);
    }

    [Fact]
    public void Debug_Writer_XOR_Issue()
    {
        // Test the writer directly
        var map = new MapData(1, 1, MapFormatType.Type1);
        map.Cells[0, 0] = new CellInfo { BackImage = 123456 };
        
        var writer = new MapWriter();
        var bytes = writer.WriteToBytes(map);
        
        System.Console.WriteLine($"Total bytes written: {bytes.Length}");
        System.Console.WriteLine($"First 60 bytes: [{string.Join(", ", bytes.Take(60).Select(b => $"0x{b:X2}"))}]");
        
        var backImageBytes = bytes.Skip(54).Take(4).ToArray();
        var actualInt32 = BitConverter.ToInt32(backImageBytes, 0);
        
        System.Console.WriteLine($"BackImage bytes at offset 54: [{string.Join(", ", backImageBytes.Select(b => $"0x{b:X2}"))}]");
        System.Console.WriteLine($"Actual int32: 0x{(uint)actualInt32:X8}");
        
        uint expected = (uint)123456 ^ 0xAA38AA38u;
        System.Console.WriteLine($"Expected: 0x{expected:X8}");
        
        Assert.Equal((int)expected, actualInt32);
    }

    [Fact]
    public void ObjectSerializer_RoundTrip_PreservesData()
    {
        // Arrange
        var originalData = new CellInfoData[]
        {
            new CellInfoData
            {
                X = 5,
                Y = 10,
                CellInfo = new CellInfo
                {
                    BackIndex = 1,
                    BackImage = 12345,
                    MiddleIndex = 2,
                    MiddleImage = 678,
                    FrontIndex = 3,
                    FrontImage = 901,
                    DoorIndex = 4,
                    DoorOffset = 5,
                    FrontAnimationFrame = 6,
                    FrontAnimationTick = 7,
                    MiddleAnimationFrame = 8,
                    MiddleAnimationTick = 9,
                    TileAnimationImage = 1234,
                    TileAnimationOffset = 5678,
                    TileAnimationFrames = 10,
                    Light = 105 // Fishing zone
                }
            },
            new CellInfoData
            {
                X = -2,
                Y = 3,
                CellInfo = new CellInfo
                {
                    BackIndex = 11,
                    BackImage = 54321,
                    Light = 50 // Normal light
                }
            }
        };

        // Act
        var serializer = new ObjectSerializer();
        var bytes = serializer.SerializeToBytes(originalData);
        var loadedData = serializer.DeserializeFromBytes(bytes);

        // Assert
        Assert.Equal(originalData.Length, loadedData.Length);

        // Check first object
        var original1 = originalData[0];
        var loaded1 = loadedData[0];
        
        Assert.Equal(original1.X, loaded1.X);
        Assert.Equal(original1.Y, loaded1.Y);
        Assert.Equal(original1.CellInfo.BackIndex, loaded1.CellInfo.BackIndex);
        Assert.Equal(original1.CellInfo.BackImage, loaded1.CellInfo.BackImage);
        Assert.Equal(original1.CellInfo.MiddleIndex, loaded1.CellInfo.MiddleIndex);
        Assert.Equal(original1.CellInfo.MiddleImage, loaded1.CellInfo.MiddleImage);
        Assert.Equal(original1.CellInfo.FrontIndex, loaded1.CellInfo.FrontIndex);
        Assert.Equal(original1.CellInfo.FrontImage, loaded1.CellInfo.FrontImage);
        Assert.Equal(original1.CellInfo.DoorIndex, loaded1.CellInfo.DoorIndex);
        Assert.Equal(original1.CellInfo.DoorOffset, loaded1.CellInfo.DoorOffset);
        Assert.Equal(original1.CellInfo.FrontAnimationFrame, loaded1.CellInfo.FrontAnimationFrame);
        Assert.Equal(original1.CellInfo.FrontAnimationTick, loaded1.CellInfo.FrontAnimationTick);
        Assert.Equal(original1.CellInfo.MiddleAnimationFrame, loaded1.CellInfo.MiddleAnimationFrame);
        Assert.Equal(original1.CellInfo.MiddleAnimationTick, loaded1.CellInfo.MiddleAnimationTick);
        Assert.Equal(original1.CellInfo.TileAnimationImage, loaded1.CellInfo.TileAnimationImage);
        Assert.Equal(original1.CellInfo.TileAnimationOffset, loaded1.CellInfo.TileAnimationOffset);
        Assert.Equal(original1.CellInfo.TileAnimationFrames, loaded1.CellInfo.TileAnimationFrames);
        Assert.Equal(original1.CellInfo.Light, loaded1.CellInfo.Light);
        Assert.Equal(original1.CellInfo.FishingCell, loaded1.CellInfo.FishingCell); // Should be true

        // Check second object
        var original2 = originalData[1];
        var loaded2 = loadedData[1];
        
        Assert.Equal(original2.X, loaded2.X);
        Assert.Equal(original2.Y, loaded2.Y);
        Assert.Equal(original2.CellInfo.BackIndex, loaded2.CellInfo.BackIndex);
        Assert.Equal(original2.CellInfo.BackImage, loaded2.CellInfo.BackImage);
        Assert.Equal(original2.CellInfo.Light, loaded2.CellInfo.Light);
        Assert.Equal(original2.CellInfo.FishingCell, loaded2.CellInfo.FishingCell); // Should be false
    }

    [Fact]
    public async Task ObjectSerializer_FileOperations_WorkCorrectly()
    {
        // Arrange
        var testData = new CellInfoData[]
        {
            new CellInfoData
            {
                X = 1,
                Y = 2,
                CellInfo = new CellInfo { Light = 110, BackImage = 999 }
            }
        };

        var tempFile = Path.GetTempFileName();
        var serializer = new ObjectSerializer();

        try
        {
            // Act - Save to file
            await serializer.SaveAsync(testData, tempFile);
            
            // Assert file exists and has correct size
            Assert.True(File.Exists(tempFile));
            var fileSize = new FileInfo(tempFile).Length;
            var expectedSize = ObjectSerializer.CalculateFileSize(1);
            Assert.Equal(expectedSize, fileSize);

            // Act - Load from file
            var loadedData = await serializer.LoadAsync(tempFile);

            // Assert data integrity
            Assert.Single(loadedData);
            Assert.Equal(testData[0].X, loadedData[0].X);
            Assert.Equal(testData[0].Y, loadedData[0].Y);
            Assert.Equal(testData[0].CellInfo.Light, loadedData[0].CellInfo.Light);
            Assert.Equal(testData[0].CellInfo.BackImage, loadedData[0].CellInfo.BackImage);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void ObjectSerializer_CalculateFileSize_ReturnsCorrectSize()
    {
        // Each object is 34 bytes + 4 bytes for count
        Assert.Equal(4, ObjectSerializer.CalculateFileSize(0)); // Just count
        Assert.Equal(4 + 34, ObjectSerializer.CalculateFileSize(1)); // Count + 1 object
        Assert.Equal(4 + (34 * 5), ObjectSerializer.CalculateFileSize(5)); // Count + 5 objects
    }
}

public class LibraryCatalogTests
{
    [Fact]
    public void LibraryCatalog_DefaultConstructor_InitializesConfig()
    {
        // Arrange & Act
        var catalog = new LibraryCatalog();

        // Assert
        Assert.NotNull(catalog.Config);
        Assert.NotEmpty(catalog.Config.LibraryPaths);
        Assert.Equal(4, catalog.Config.LibraryPaths.Count);
        Assert.True(catalog.Config.AutoScanOnStartup);
    }

    [Fact]
    public void LibraryCatalog_CustomConfig_UsesProvidedConfig()
    {
        // Arrange
        var customConfig = new LibraryConfig
        {
            AutoScanOnStartup = false,
            ObjectsPath = @"C:\TestObjects\"
        };

        // Act
        var catalog = new LibraryCatalog(customConfig);

        // Assert
        Assert.Same(customConfig, catalog.Config);
        Assert.False(catalog.Config.AutoScanOnStartup);
        Assert.Equal(@"C:\TestObjects\", catalog.Config.ObjectsPath);
    }

    [Fact]
    public void LibraryItem_ToString_ReturnsName()
    {
        // Arrange
        var item = new LibraryItem { Name = "TestLibrary", Index = 42 };

        // Act & Assert
        Assert.Equal("TestLibrary", item.ToString());
    }

    [Fact]
    public async Task LibraryCatalog_ScanLibrariesAsync_CompletesWithoutException()
    {
        // Arrange
        var catalog = new LibraryCatalog();

        // Act & Assert - Should complete without throwing
        await catalog.ScanLibrariesAsync();
        
        // Libraries dictionary should be initialized (empty is fine since no actual lib files exist)
        Assert.NotNull(catalog.Libraries);
    }

    [Fact]
    public void LibraryCatalog_GetLibrary_NonExistentIndex_ReturnsNull()
    {
        // Arrange
        var catalog = new LibraryCatalog();

        // Act
        var library = catalog.GetLibrary(999);

        // Assert
        Assert.Null(library);
    }

    [Fact]
    public void LibraryCatalog_GetLibrariesByType_ReturnsFilteredResults()
    {
        // Arrange
        var catalog = new LibraryCatalog();

        // Act
        var wemadeMir2Libraries = catalog.GetLibrariesByType(LibraryType.WemadeMir2);

        // Assert
        Assert.NotNull(wemadeMir2Libraries);
        // Should be empty since no actual library files exist
        Assert.Empty(wemadeMir2Libraries);
    }

    [Theory]
    [InlineData(LibraryType.WemadeMir2)]
    [InlineData(LibraryType.ShandaMir2)]
    [InlineData(LibraryType.WemadeMir3)]
    [InlineData(LibraryType.ShandaMir3)]
    public void LibraryConfig_DefaultPaths_ContainsAllTypes(LibraryType type)
    {
        // Arrange
        var config = new LibraryConfig();

        // Act & Assert
        Assert.True(config.LibraryPaths.ContainsKey(type));
        Assert.NotEmpty(config.LibraryPaths[type]);
    }

    [Fact]
    public async Task LibraryCatalog_SaveAndLoadConfig_RoundTrip()
    {
        // Arrange
        var originalConfig = new LibraryConfig
        {
            AutoScanOnStartup = false,
            ObjectsPath = @"C:\TestPath\"
        };
        originalConfig.LibraryPaths[LibraryType.WemadeMir2] = @"C:\CustomMir2\";

        var catalog = new LibraryCatalog(originalConfig);
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            await catalog.SaveConfigAsync(tempFile);
            var loadedConfig = await LibraryCatalog.LoadConfigAsync(tempFile);

            // Assert
            Assert.Equal(originalConfig.AutoScanOnStartup, loadedConfig.AutoScanOnStartup);
            Assert.Equal(originalConfig.ObjectsPath, loadedConfig.ObjectsPath);
            Assert.Equal(originalConfig.LibraryPaths[LibraryType.WemadeMir2], loadedConfig.LibraryPaths[LibraryType.WemadeMir2]);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LibraryCatalog_LoadConfig_NonExistentFile_ReturnsDefaultConfig()
    {
        // Arrange
        var nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");

        // Act
        var config = await LibraryCatalog.LoadConfigAsync(nonExistentFile);

        // Assert
        Assert.NotNull(config);
        Assert.True(config.AutoScanOnStartup);
        Assert.Equal(@".\Data\Objects\", config.ObjectsPath);
    }
}