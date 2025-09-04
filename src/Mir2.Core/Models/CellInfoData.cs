namespace Mir2.Core.Models;

/// <summary>
/// Represents cell information with coordinates for editing operations
/// </summary>
public class CellInfoData
{
    /// <summary>
    /// X coordinate in the map
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Y coordinate in the map
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// The cell information
    /// </summary>
    public CellInfo CellInfo { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public CellInfoData()
    {
        CellInfo = new CellInfo();
    }

    /// <summary>
    /// Constructor with coordinates and cell info
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="cellInfo">Cell information to copy</param>
    public CellInfoData(int x, int y, CellInfo cellInfo)
    {
        X = x;
        Y = y;
        CellInfo = cellInfo?.Clone() ?? new CellInfo();
    }
}