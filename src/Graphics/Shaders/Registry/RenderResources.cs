namespace ChangeTrace.Graphics.Shaders.Registry;

/// <summary>
/// Central registry for shaders, renderers and GPU render targets.
/// </summary>
internal sealed class RenderResources : IDisposable
{
    /// <summary>
    /// Shared shader resource registry.
    /// </summary>
    public ShaderResources Shaders { get; } =
        new();

    /// <summary>
    /// Renderer resource registry.
    /// </summary>
    public RendererResources Renderers { get; }

    /// <summary>
    /// GPU render target registry.
    /// </summary>
    public TargetResources Targets { get; }

    public RenderResources(
        string fontAtlasPath,
        int viewportW,
        int viewportH)
    {
        Renderers =
            new RendererResources(
                fontAtlasPath);

        Targets =
            new TargetResources(
                viewportW,
                viewportH,
                Shaders);
    }

    /// <summary>
    /// Resizes GPU render targets to match the viewport.
    /// </summary>
    public void ResizeGpuTargets(
        int viewportW,
        int viewportH)
    {
        Targets.Resize(
            viewportW,
            viewportH,
            Shaders);
    }

    public void Dispose()
    {
        Renderers.Dispose();
        Targets.Dispose();
        Shaders.Dispose();
    }
}