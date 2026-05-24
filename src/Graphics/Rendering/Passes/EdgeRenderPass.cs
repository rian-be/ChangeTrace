using ChangeTrace.Graphics.Gpu.Pipelines;
using ChangeTrace.Graphics.Rendering.Runtime;
using ChangeTrace.Graphics.Rendering.Scene;
using ChangeTrace.Rendering.Snapshots;

namespace ChangeTrace.Graphics.Rendering.Passes;

/// <summary>
/// Renders visible hierarchy edges between scene nodes.
/// </summary>
internal sealed class EdgeRenderPass(EdgeGpuPipeline pipeline, SceneVisibilitySystem visibility) : RenderPass
{
    private IReadOnlyList<NodeSnapshot> VisibleNodes { get; set; } =  [];

    /// <summary>
    /// Updates the node set used for edge generation.
    /// </summary>
    public void SetVisibleNodes(
        IReadOnlyList<NodeSnapshot> nodes) =>
        VisibleNodes = nodes;

    /// <summary>
    /// Executes the edge render pass.
    /// </summary>
    public override void Render(in RenderFrameContext context)
    {
        if (VisibleNodes.Count == 0)
            return;

        // Build hierarchy edges only for currently visible nodes.
        var edges = visibility.BuildVisibleHierarchyEdges(context.State, VisibleNodes);
        if (edges.Count == 0)
            return;

        int count = pipeline.UploadEdges(edges, context.State.Scene, context.Zoom);

        if (count <= 0)
            return;

        // GPU culling + visible edge compaction.
        pipeline.Execute(
            context.Resources.Shaders.Compute("EdgeCull"),
            count,
            context.ViewProjection,
            context.Viewport,
            context.Zoom);

        // Render compacted edges using indirect draw.
        context.Resources.Renderers.Edges.Draw(
            pipeline,
            context.Resources.Shaders.Graphics("Edge"),
            context.ViewProjection);
    }
}