namespace Mir2.Core.Models;

/// <summary>
/// Represents a single cell in a Mir2 map with all its properties
/// </summary>
public class CellInfo
{
    /// <summary>
    /// Library index for background tile (资源编号)
    /// </summary>
    public short BackIndex { get; set; }

    /// <summary>
    /// Image index within the background library (资源内图片索引)
    /// </summary>
    public int BackImage { get; set; }

    /// <summary>
    /// Library index for middle layer tile
    /// </summary>
    public short MiddleIndex { get; set; }

    /// <summary>
    /// Image index within the middle layer library
    /// </summary>
    public short MiddleImage { get; set; }

    /// <summary>
    /// Library index for front layer tile
    /// </summary>
    public short FrontIndex { get; set; }

    /// <summary>
    /// Image index within the front layer library
    /// </summary>
    public short FrontImage { get; set; }

    /// <summary>
    /// Door index (0 = no door, 1-255 = door types)
    /// </summary>
    public byte DoorIndex { get; set; }

    /// <summary>
    /// Door offset for animation
    /// </summary>
    public byte DoorOffset { get; set; }

    /// <summary>
    /// Animation frame for front layer
    /// </summary>
    public byte FrontAnimationFrame { get; set; }

    /// <summary>
    /// Animation tick for front layer
    /// </summary>
    public byte FrontAnimationTick { get; set; }

    /// <summary>
    /// Animation frame for middle layer
    /// </summary>
    public byte MiddleAnimationFrame { get; set; }

    /// <summary>
    /// Animation tick for middle layer
    /// </summary>
    public byte MiddleAnimationTick { get; set; }

    /// <summary>
    /// Tile animation image index
    /// </summary>
    public short TileAnimationImage { get; set; }

    /// <summary>
    /// Tile animation offset
    /// </summary>
    public short TileAnimationOffset { get; set; }

    /// <summary>
    /// Number of tile animation frames
    /// </summary>
    public byte TileAnimationFrames { get; set; }

    /// <summary>
    /// Light level (0 = no light, 1-99 = light levels, 100-101 = fishing zones)
    /// </summary>
    public byte Light { get; set; }

    /// <summary>
    /// Unknown field for future use
    /// </summary>
    public byte Unknown { get; set; }

    /// <summary>
    /// True if this cell is a fishing zone (derived from Light 100-119 range)
    /// Legacy reference: MapCode.cs sets FishingCell when Light == 100 || Light == 101
    /// Extended to 100-119 range as per compatibility requirements
    /// </summary>
    public bool FishingCell => Light >= 100 && Light <= 119;

    /// <summary>
    /// Creates a deep copy of the CellInfo
    /// </summary>
    public CellInfo Clone()
    {
        return new CellInfo
        {
            BackIndex = BackIndex,
            BackImage = BackImage,
            MiddleIndex = MiddleIndex,
            MiddleImage = MiddleImage,
            FrontIndex = FrontIndex,
            FrontImage = FrontImage,
            DoorIndex = DoorIndex,
            DoorOffset = DoorOffset,
            FrontAnimationFrame = FrontAnimationFrame,
            FrontAnimationTick = FrontAnimationTick,
            MiddleAnimationFrame = MiddleAnimationFrame,
            MiddleAnimationTick = MiddleAnimationTick,
            TileAnimationImage = TileAnimationImage,
            TileAnimationOffset = TileAnimationOffset,
            TileAnimationFrames = TileAnimationFrames,
            Light = Light,
            Unknown = Unknown
        };
    }
}
