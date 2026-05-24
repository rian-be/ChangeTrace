using ChangeTrace.Graphics.Gpu.Pipelines;
using ChangeTrace.Graphics.Rendering.Runtime;
using ChangeTrace.Graphics.Rendering.Scene;
using OpenTK.Graphics.OpenGL4;

namespace ChangeTrace.Graphics.Rendering.Passes;

/// <summary>
/// Renders the activity heatmap overlay.
/// </summary>
internal sealed class HeatmapRenderPass(HeatmapGpuPipeline pipeline, HeatmapCollector collector, int maxObjects) : RenderPass
{
    /// <summary>
    /// Heatmap accumulation intensity multiplier.
    /// </summary>
    private float Intensity { get; set; } = 2.4f;

    /// <summary>
    /// Executes the heatmap render pass.
    /// </summary>
    public override void Render(in RenderFrameContext context)
    {
        var objects =
            collector.Collect(
                context.State,
                context.ViewProjection,
                pipeline.Size,
                maxObjects);

        if (objects.Length == 0)
            return;

        // Upload heatmap influence objects.
        pipeline.UpdateObjectData(objects);

        // Generate heatmap texture on GPU.
        pipeline.Generate(Intensity);
        GL.Enable(EnableCap.Blend);

        // Additive blend for heatmap glows.
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);

        context.Resources.Renderers.HeatmapTexture.Draw(
            pipeline.Texture,
            context.Resources.Shaders.Graphics("HeatmapTexture"));

        // Restore standard alpha blending.
        GL.BlendFunc(
            BlendingFactor.SrcAlpha,
            BlendingFactor.OneMinusSrcAlpha);
    }
}