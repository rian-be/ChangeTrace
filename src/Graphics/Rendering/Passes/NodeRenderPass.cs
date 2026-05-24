using ChangeTrace.Graphics.Gpu.Pipelines;
using ChangeTrace.Graphics.Rendering.Runtime;
using ChangeTrace.Graphics.Rendering.Scene;
using ChangeTrace.Rendering.Snapshots;

namespace ChangeTrace.Graphics.Rendering.Passes;

/// <summary>
/// Renders visible scene nodes.
/// </summary>
internal sealed class NodeRenderPass(CircleGpuPipeline pipeline, SceneVisibilitySystem visibility)
    : RenderPass
{
    private readonly List<NodeSnapshot> _branchNodes = [];
    private readonly List<NodeSnapshot> _fileNodes = [];

    private IReadOnlyList<NodeSnapshot> VisibleNodes { get; set; } =  [];

    /// <summary>
    /// Updates the node set used by this pass.
    /// </summary>
    public void SetVisibleNodes(
        IReadOnlyList<NodeSnapshot> nodes) =>
        VisibleNodes =
            nodes;

    /// <summary>
    /// Executes the node render pass.
    /// </summary>
    public override void Render(in RenderFrameContext context)
    {
        if (VisibleNodes.Count == 0)
            return;

        // Draw folders/branches before files.
        SceneVisibilitySystem.SplitNodes(VisibleNodes, _branchNodes, _fileNodes);

        DrawBatch(context, _branchNodes);
        DrawBatch(context, _fileNodes);
    }

    /// <summary>
    /// Uploads, culls, and renders one node batch.
    /// </summary>
    private void DrawBatch(
        in RenderFrameContext context,
        IReadOnlyList<NodeSnapshot> nodes)
    {
        if (nodes.Count == 0)
            return;

        int count = pipeline.UploadNodes(nodes, context.Zoom);

        if (count <= 0)
            return;

        // GPU node culling + visible node compaction.
        pipeline.Execute(
            context.Resources.Shaders.Compute("CircleCull"),
            count,
            context.ViewProjection,
            context.Viewport,
            context.Zoom);

        var shader = context.Resources.Shaders.Graphics("Circle");

        shader.Use();
        shader.Set("uTime", context.TotalTime);

        // Render compacted visible nodes using indirect draw.
        context.Resources.Renderers.Circles.DrawGpu(
            pipeline,
            shader,
            context.ViewProjection);
    }
}