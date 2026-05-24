using ChangeTrace.Graphics.Gpu.Pipelines;
using ChangeTrace.Graphics.Rendering.Runtime;

namespace ChangeTrace.Graphics.Rendering.Passes;

/// <summary>
/// Renders scene particles using the GPU particle pipeline.
/// </summary>
internal sealed class ParticleRenderPass(ParticleGpuPipeline pipeline) : RenderPass
{
    /// <summary>
    /// Executes the particle render pass.
    /// </summary>
    public override void Render(in RenderFrameContext context)
    {
        int count = pipeline.UploadParticles(context.State.Scene.Particles);

        if (count <= 0)
            return;

        // GPU particle culling + visible particle compaction.
        pipeline.Execute(
            context.Resources.Shaders.Compute("ParticleCull"),
            count,
            context.ViewProjection,
            context.Viewport,
            context.Zoom);

        // Render compacted visible particles.
        context.Resources.Renderers.Particles.Draw(
            context.Resources.Shaders.Graphics("Particle"),
            pipeline,
            context.ViewProjection);
    }
}