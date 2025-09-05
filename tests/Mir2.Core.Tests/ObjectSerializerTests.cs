using Xunit;
using Mir2.Core.IO;
using Mir2.Core.Models;
using System.IO;
using System.Threading.Tasks;

namespace Mir2.Core.Tests;

/// <summary>
/// Tests for ObjectSerializer functionality from archived files
/// </summary>
public class ObjectSerializerTests
{
    [Fact]
    public async Task ObjectSerializer_SaveAndLoad_RoundTrip()
    {
        // Arrange
        var serializer = new ObjectSerializer();
        var originalData = new CellInfoData[]
        {
            new CellInfoData(1, 2, new CellInfo
            {
                BackIndex = 10,
                BackImage = 100,
                MiddleIndex = 20,
                MiddleImage = 200,
                FrontIndex = 30,
                FrontImage = 300,
                DoorIndex = 5,
                DoorOffset = 15,
                Light = 50
            }),
            new CellInfoData(3, 4, new CellInfo
            {
                BackIndex = 11,
                BackImage = 101,
                TileAnimationFrames = 3,
                Light = 100 // Fishing zone
            })
        };

        var tempFile = Path.GetTempFileName();
        
        try
        {
            // Act - Save and Load
            await serializer.SaveAsync(originalData, tempFile);
            var loadedData = await serializer.LoadAsync(tempFile);

            // Assert
            Assert.Equal(originalData.Length, loadedData.Length);
            
            for (int i = 0; i < originalData.Length; i++)
            {
                var orig = originalData[i];
                var loaded = loadedData[i];
                
                Assert.Equal(orig.X, loaded.X);
                Assert.Equal(orig.Y, loaded.Y);
                Assert.Equal(orig.CellInfo.BackIndex, loaded.CellInfo.BackIndex);
                Assert.Equal(orig.CellInfo.BackImage, loaded.CellInfo.BackImage);
                Assert.Equal(orig.CellInfo.MiddleIndex, loaded.CellInfo.MiddleIndex);
                Assert.Equal(orig.CellInfo.MiddleImage, loaded.CellInfo.MiddleImage);
                Assert.Equal(orig.CellInfo.FrontIndex, loaded.CellInfo.FrontIndex);
                Assert.Equal(orig.CellInfo.FrontImage, loaded.CellInfo.FrontImage);
                Assert.Equal(orig.CellInfo.DoorIndex, loaded.CellInfo.DoorIndex);
                Assert.Equal(orig.CellInfo.DoorOffset, loaded.CellInfo.DoorOffset);
                Assert.Equal(orig.CellInfo.Light, loaded.CellInfo.Light);
                Assert.Equal(orig.CellInfo.TileAnimationFrames, loaded.CellInfo.TileAnimationFrames);
                
                // Verify FishingCell is computed correctly
                Assert.Equal(orig.CellInfo.FishingCell, loaded.CellInfo.FishingCell);
            }
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void ObjectSerializer_CalculateFileSize_CorrectCalculation()
    {
        // Arrange & Act
        var size = ObjectSerializer.CalculateFileSize(10);
        
        // Assert - 4 bytes for count + (10 objects * 34 bytes per object)
        Assert.Equal(344, size);
    }

    [Fact]
    public async Task ObjectSerializer_LoadNonExistentFile_ReturnsEmpty()
    {
        // Arrange
        var serializer = new ObjectSerializer();
        var nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        
        // Act
        var result = await serializer.LoadAsync(nonExistentFile);
        
        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ObjectSerializer_SerializeEmpty_ValidResult()
    {
        // Arrange
        var serializer = new ObjectSerializer();
        var emptyData = new CellInfoData[0];
        
        // Act
        var bytes = serializer.SerializeToBytes(emptyData);
        
        // Assert - Should have 4 bytes for count (0)
        Assert.Equal(4, bytes.Length);
        Assert.Equal(0, BitConverter.ToInt32(bytes, 0));
    }
}