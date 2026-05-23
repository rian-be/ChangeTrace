using ChangeTrace.Graphics.Gpu.Targets;
using ChangeTrace.Graphics.Shaders.Registry;

namespace ChangeTrace.Graphics.Shaders;

/// <summary>
/// Central registry for GPU render targets used by the rendering pipeline.
/// </summary>
internal sealed class TargetResources(
    int viewportW,
    int viewportH,
    ShaderResources shaders)
    : IDisposable
{
    /// <summary>
    /// Bloom mask render target.
    /// </summary>
    public BloomMask BloomMask { get; private set; } =
        CreateBloom(
            viewportW,
            viewportH,
            shaders);

    /// <summary>
    /// Recreates render targets for a resized viewport.
    /// </summary>
    public void Resize(
        int viewportW,
        int viewportH,
        ShaderResources shaders)
    {
        BloomMask.Dispose();

        BloomMask =
            CreateBloom(
                viewportW,
                viewportH,
                shaders);
    }

    /// <summary>
    /// Creates bloom mask render target.
    /// </summary>
    private static BloomMask CreateBloom(
        int width,
        int height,
        ShaderResources shaders)
    {
        return new BloomMask(
            width,
            height,
            shaders.Compute("BloomMask"));
    }

    public void Dispose() =>
        BloomMask.Dispose();
}