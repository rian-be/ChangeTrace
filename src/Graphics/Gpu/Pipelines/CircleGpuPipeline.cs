using ChangeTrace.Graphics.Gpu.Contracts;
using ChangeTrace.Graphics.Gpu.Pipelines.Base;
using ChangeTrace.Graphics.Shaders.Runtime;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Snapshots;
using OpenTK.Graphics.OpenGL4;
using TkMatrix3 = OpenTK.Mathematics.Matrix3;
using TkVector2 = OpenTK.Mathematics.Vector2;
using GpuVector2 = System.Numerics.Vector2;
using GpuVector4 = System.Numerics.Vector4;

namespace ChangeTrace.Graphics.Gpu.Pipelines;

/// <summary>
/// GPU pipeline for circle/node rendering.
/// Handles node upload, visibility compaction, and indirect rendering.
/// </summary>
internal sealed class CircleGpuPipeline
    : IndirectComputePipeline<GpuCircleNode>
{
    /// <summary>
    /// GPU node upload capacity.
    /// </summary>
    private const int MaxNodes = 65536;

    /// <summary>
    /// Minimum allowed zoom factor.
    /// Prevents division instability.
    /// </summary>
    private const float MinZoom = 0.001f;

    /// <summary>
    /// Base screen space radii.
    /// </summary>
    private const float FileScreenRadius = 4.8f;
    private const float BranchScreenRadius = 8.5f;
    private const float RootScreenRadius = 13.5f;

    /// <summary>
    /// Screen space radius clamps.
    /// </summary>
    private const float FileMinScreenRadius = 3.2f;
    private const float FileMaxScreenRadius = 7.5f;

    private const float BranchMinScreenRadius = 6.0f;
    private const float BranchMaxScreenRadius = 13.0f;

    private const float RootMinScreenRadius = 10.0f;
    private const float RootMaxScreenRadius = 18.0f;

    /// <summary>
    /// Controls how strongly node size reacts to zoom.
    /// Lower values keep nodes visually stable.
    /// </summary>
    private const float ZoomRadiusResponse = 0.04f;

    private readonly GpuCircleNode[] _cpuUpload =
        new GpuCircleNode[MaxNodes];

    /// <summary>
    /// Visible compacted node buffer handle.
    /// </summary>
    public int VisibleInstanceBuffer =>
        VisibleStorageBuffer.Handle;

    /// <summary>
    /// Indirect draw command buffer handle.
    /// </summary>
    public int IndirectBuffer =>
        IndirectDrawBuffer.Handle;

    /// <summary>
    /// Source node buffer handle.
    /// </summary>
    public int InputInstanceBuffer =>
        InputStorageBuffer.Handle;

    public CircleGpuPipeline()
    {
        Initialize(
            maxItems: MaxNodes,
            vertexCountPerInstance: 6,
            inputUsage: BufferUsageHint.StreamDraw);
    }

    /// <summary>
    /// Uploads node instances into GPU storage.
    /// </summary>
    public int UploadNodes(
        IReadOnlyList<NodeSnapshot> nodes,
        float zoom)
    {
        int count =
            Math.Min(
                nodes.Count,
                MaxNodes);

        if (count <= 0)
            return 0;

        float safeZoom =
            MathF.Max(
                zoom,
                MinZoom);

        float zoomGrow =
            MathF.Pow(
                safeZoom,
                ZoomRadiusResponse);

        for (int i = 0; i < count; i++)
        {
            NodeSnapshot node = nodes[i];

            GetRadiusProfile(
                node,
                out float baseScreenRadius,
                out float minScreenRadius,
                out float maxScreenRadius);

            float screenRadius =
                Math.Clamp(
                    baseScreenRadius * zoomGrow,
                    minScreenRadius,
                    maxScreenRadius);

            float worldRadius =
                screenRadius / safeZoom;

            float alpha =
                ResolveAlpha(node);

            float glow =
                ResolveGlow(node);

            _cpuUpload[i] = new GpuCircleNode
            {
                Position = new GpuVector2(
                    node.Position.X,
                    node.Position.Y),
                Radius = worldRadius,
                Kind = (float)node.Kind,
                Color = node.Color with { W = alpha },
                Glow = glow,
                Pad0 = 0f,
                Pad1 = 0f,
                Pad2 = 0f
            };
        }

        InputStorageBuffer.SubData(
            _cpuUpload.AsSpan(0, count));

        return count;
    }

    /// <summary>
    /// Executes compute visibility pass for uploaded nodes.
    /// </summary>
    public void Execute(
        ComputeShader computeShader,
        int nodeCount,
        TkMatrix3 viewProj,
        TkVector2 viewport,
        float zoom)
    {
        if (nodeCount <= 0)
            return;

        ResetIndirect();

        computeShader.Use();
        computeShader.Set("uNodeCount", nodeCount);
        computeShader.Set("uViewProj", viewProj);
        computeShader.Set("uViewport", viewport);
        computeShader.Set("uZoom", zoom);

        BindCommonBuffers();
        Dispatch256(nodeCount);
    }

    /// <summary>
    /// Resolves screen-space radius limits for a node type.
    /// </summary>
    private static void GetRadiusProfile(
        NodeSnapshot node,
        out float baseScreenRadius,
        out float minScreenRadius,
        out float maxScreenRadius)
    {
        if (node.Kind == NodeKind.File)
        {
            baseScreenRadius =
                MathF.Max(
                    FileScreenRadius,
                    node.Radius * 0.75f);

            minScreenRadius = FileMinScreenRadius;
            maxScreenRadius = FileMaxScreenRadius;

            return;
        }

        if (node.Kind == NodeKind.Root)
        {
            baseScreenRadius =
                MathF.Max(
                    RootScreenRadius,
                    node.Radius * 0.70f);

            minScreenRadius = RootMinScreenRadius;
            maxScreenRadius = RootMaxScreenRadius;

            return;
        }

        baseScreenRadius =
            MathF.Max(
                BranchScreenRadius,
                node.Radius * 0.75f);

        minScreenRadius = BranchMinScreenRadius;
        maxScreenRadius = BranchMaxScreenRadius;
    }

    /// <summary>
    /// Resolves final node alpha used for rendering.
    /// </summary>
    private static float ResolveAlpha(NodeSnapshot node)
    {
        return node.Kind switch
        {
            NodeKind.File => MathF.Max(node.Color.W, 0.92f),
            NodeKind.Branch => MathF.Max(node.Color.W, 0.95f),
            NodeKind.Root => 1.0f,
            _ => MathF.Max(node.Color.W, 0.90f)
        };
    }

    /// <summary>
    /// Resolves final node glow intensity.
    /// </summary>
    private static float ResolveGlow(NodeSnapshot node)
    {
        return node.Kind switch
        {
            NodeKind.File => node.Glow * 0.45f,
            NodeKind.Branch => MathF.Max(node.Glow, 0.10f),
            NodeKind.Root => MathF.Max(node.Glow, 0.65f),
            _ => node.Glow
        };
    }
}