using Mir2.Core.Models;

namespace Mir2.Render;

/// <summary>
/// Renderer interface for rendering maps and overlays
/// </summary>
public interface IMapRenderer
{
    /// <summary>
    /// Initializes the renderer
    /// </summary>
    /// <param name="width">Viewport width</param>
    /// <param name="height">Viewport height</param>
    void Initialize(int width, int height);

    /// <summary>
    /// Renders a map with optional overlays
    /// </summary>
    /// <param name="mapData">Map data to render</param>
    /// <param name="options">Rendering options</param>
    void RenderMap(MapData mapData, RenderOptions options);

    /// <summary>
    /// Resizes the renderer viewport
    /// </summary>
    /// <param name="width">New width</param>
    /// <param name="height">New height</param>
    void Resize(int width, int height);

    /// <summary>
    /// Disposes of resources
    /// </summary>
    void Dispose();
}

/// <summary>
/// Options for rendering
/// </summary>
public class RenderOptions
{
    /// <summary>
    /// Whether to show the back layer
    /// </summary>
    public bool ShowBackLayer { get; set; } = true;

    /// <summary>
    /// Whether to show the middle layer
    /// </summary>
    public bool ShowMiddleLayer { get; set; } = true;

    /// <summary>
    /// Whether to show the front layer
    /// </summary>
    public bool ShowFrontLayer { get; set; } = true;

    /// <summary>
    /// Whether to show grid lines
    /// </summary>
    public bool ShowGrid { get; set; } = false;

    /// <summary>
    /// Whether to show light overlays
    /// </summary>
    public bool ShowLightOverlay { get; set; } = false;

    /// <summary>
    /// Whether to show door masks
    /// </summary>
    public bool ShowDoorMasks { get; set; } = false;

    /// <summary>
    /// Whether to show animation frames
    /// </summary>
    public bool ShowAnimations { get; set; } = true;

    /// <summary>
    /// Zoom level (1.0 = normal, 2.0 = 2x zoom, etc.)
    /// </summary>
    public float ZoomLevel { get; set; } = 1.0f;

    /// <summary>
    /// Camera offset X
    /// </summary>
    public int OffsetX { get; set; } = 0;

    /// <summary>
    /// Camera offset Y
    /// </summary>
    public int OffsetY { get; set; } = 0;
}
