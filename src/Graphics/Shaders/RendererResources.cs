using ChangeTrace.Graphics.Rendering.Renderers.Effects;
using ChangeTrace.Graphics.Rendering.Renderers.Scene;
using ChangeTrace.Graphics.Rendering.Renderers.Text;

namespace ChangeTrace.Graphics.Shaders;

/// <summary>
/// Central registry for renderer instances used by the rendering pipeline.
/// </summary>
internal sealed class RendererResources(string fontAtlasPath) : IDisposable
{
    /// <summary>
    /// Circle/node renderer.
    /// </summary>
    public CircleRenderer Circles { get; } =
        new();

    /// <summary>
    /// Edge renderer.
    /// </summary>
    public EdgeRenderer Edges { get; } =
        new();

    /// <summary>
    /// Pawn/avatar renderer.
    /// </summary>
    public PawnRenderer Pawns { get; } =
        new();

    /// <summary>
    /// Particle renderer.
    /// </summary>
    public ParticleRenderer Particles { get; } =
        new();

    /// <summary>
    /// Heatmap texture renderer.
    /// </summary>
    public HeatmapTextureRenderer HeatmapTexture { get; } =
        new();

    /// <summary>
    /// Fullscreen background renderer.
    /// </summary>
    public BackgroundRenderer Background { get; } =
        new();

    /// <summary>
    /// Bitmap text renderer.
    /// </summary>
    public TextRenderer Text { get; } =
        new(fontAtlasPath);

    public void Dispose()
    {
        Circles.Dispose();
        Edges.Dispose();

        Pawns.Dispose();
        Particles.Dispose();

        HeatmapTexture.Dispose();
        Background.Dispose();

        Text.Dispose();
    }
}