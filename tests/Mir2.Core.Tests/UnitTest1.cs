using Mir2.Core.IO;
using Mir2.Core.Models;

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

        // Act & Assert
        Assert.False(cellNormal.FishingCell);
        Assert.True(cellFishing100.FishingCell);
        Assert.True(cellFishing101.FishingCell);
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
}