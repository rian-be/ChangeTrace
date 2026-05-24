using ChangeTrace.Graphics.Gpu.Contracts;
using ChangeTrace.Graphics.Gpu.Pipelines.Base;
using ChangeTrace.Graphics.Shaders.Runtime;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Snapshots;
using OpenTK.Mathematics;
using GpuVector2 = System.Numerics.Vector2;
using GpuVector4 = System.Numerics.Vector4;
using TkMatrix3 = OpenTK.Mathematics.Matrix3;

namespace ChangeTrace.Graphics.Gpu.Pipelines;

/// <summary>
/// GPU pipeline for edge rendering.
/// Handles edge upload, visibility compaction and indirect rendering.
/// </summary>
internal sealed class EdgeGpuPipeline
    : IndirectComputePipeline<GpuEdge>
{
    /// <summary>
    /// GPU edge upload capacity.
    /// </summary>
    private const int MaxEdges = 65536;

    /// <summary>
    /// Minimum allowed zoom factor.
    /// Prevents division instability.
    /// </summary>
    private const float MinZoom = 0.001f;

    /// <summary>
    /// Minimum visible edge thickness in screen pixels.
    /// </summary>
    private const float MinWidthPixels = 0.35f;

    /// <summary>
    /// Small trim applied near node centers.
    /// Prevents edges from visually overshooting circles.
    /// </summary>
    private const float NodeEdgePadding = 2.0f;

    /// <summary>
    /// Hierarchy edge length multiplier.
    /// </summary>
    private const float HierarchyEdgeLengthFactor = 1.0f;

    /// <summary>
    /// Alpha reduction applied to edges targeting file nodes.
    /// </summary>
    private const float FileEdgeAlphaMultiplier = 0.75f;

    private readonly GpuEdge[] _cpuUpload =
        new GpuEdge[MaxEdges];

    /// <summary>
    /// Visible compacted edge buffer handle.
    /// </summary>
    public int VisibleEdgeBuffer =>
        VisibleStorageBuffer.Handle;

    /// <summary>
    /// Indirect draw command buffer handle.
    /// </summary>
    public int IndirectBuffer =>
        IndirectDrawBuffer.Handle;

    public EdgeGpuPipeline()
    {
        Initialize(
            maxItems: MaxEdges,
            vertexCountPerInstance: 6);
    }

    /// <summary>
    /// Uploads edges into GPU storage.
    /// </summary>
    public int UploadEdges(
        IReadOnlyList<EdgeSnapshot> edges,
        ISceneSnapshot scene,
        float zoom)
    {
        int maxCount =
            Math.Min(
                edges.Count,
                MaxEdges);

        int written = 0;

        float safeZoom =
            MathF.Max(
                zoom,
                MinZoom);

        foreach (EdgeSnapshot edge in edges)
        {
            if (written >= maxCount)
                break;

            NodeSnapshot? from =
                scene.FindNode(edge.FromId);

            NodeSnapshot? to =
                scene.FindNode(edge.ToId);

            if (from == null || to == null)
                continue;

            NodeSnapshot fromNode = from.Value;
            NodeSnapshot toNode = to.Value;

            Vector2 fromPos = new(
                fromNode.Position.X,
                fromNode.Position.Y);

            Vector2 toPos = new(
                toNode.Position.X,
                toNode.Position.Y);

            Vector2 delta = toPos - fromPos;

            float length = delta.Length;

            if (length < 0.001f)
                continue;

            Vector2 dir = delta / length;

            float fromTrim =
                MathF.Max(
                    0f,
                    NodeEdgePadding);

            float toTrim =
                MathF.Max(
                    0f,
                    NodeEdgePadding);

            float maxTrim =
                length * 0.20f;

            fromTrim =
                MathF.Min(
                    fromTrim,
                    maxTrim);

            toTrim =
                MathF.Min(
                    toTrim,
                    maxTrim);

            Vector2 clippedFrom =
                fromPos + dir * fromTrim;

            float visibleLength =
                MathF.Max(
                    0f,
                    length - fromTrim - toTrim);

            float lengthFactor =
                edge.Kind == EdgeKind.Hierarchy
                    ? HierarchyEdgeLengthFactor
                    : 1.0f;

            float finalLength =
                visibleLength * lengthFactor;

            Vector2 clippedTo =
                clippedFrom + dir * finalLength;

            float widthStart = edge.WidthStart;
            float widthEnd = edge.WidthEnd;

            if (widthStart * safeZoom < MinWidthPixels)
                widthStart = MinWidthPixels / safeZoom;

            if (widthEnd * safeZoom < MinWidthPixels)
                widthEnd = MinWidthPixels / safeZoom;

            float alpha = edge.Alpha;

            if (toNode.Kind == NodeKind.File)
            {
                alpha *= FileEdgeAlphaMultiplier;

                widthStart *= 0.75f;
                widthEnd *= 0.75f;
            }

            if (toNode.Kind != NodeKind.File)
            {
                alpha =
                    MathF.Max(
                        alpha,
                        0.12f);
            }

            _cpuUpload[written++] = new GpuEdge
            {
                From = new GpuVector2(
                    clippedFrom.X,
                    clippedFrom.Y),

                To = new GpuVector2(
                    clippedTo.X,
                    clippedTo.Y),

                Color = new GpuVector4(
                    edge.Color.X,
                    edge.Color.Y,
                    edge.Color.Z,
                    edge.Color.W),

                Alpha = alpha,

                WidthStart = widthStart,
                WidthEnd = widthEnd,

                Kind = (float)edge.Kind,

                FromGlow = fromNode.Glow,
                ToGlow = toNode.Glow
            };
        }

        if (written <= 0)
            return 0;

        InputStorageBuffer.SubData(
            _cpuUpload.AsSpan(0, written));

        return written;
    }

    /// <summary>
    /// Uploads prebuilt GPU edges directly into GPU storage.
    /// </summary>
    public int UploadGpuEdges(
        IReadOnlyList<GpuEdge> edges)
    {
        int count =
            Math.Min(
                edges.Count,
                MaxEdges);

        if (count <= 0)
            return 0;

        for (int i = 0; i < count; i++)
            _cpuUpload[i] = edges[i];

        InputStorageBuffer.SubData(
            _cpuUpload.AsSpan(0, count));

        return count;
    }

    /// <summary>
    /// Executes compute visibility pass for uploaded edges.
    /// </summary>
    public void Execute(
        ComputeShader computeShader,
        int edgeCount,
        TkMatrix3 viewProj,
        Vector2 viewport,
        float zoom)
    {
        if (edgeCount <= 0)
            return;

        ResetIndirect();

        computeShader.Use();
        computeShader.Set("uEdgeCount", edgeCount);
        computeShader.Set("uViewProj", viewProj);
        computeShader.Set("uViewport", viewport);
        computeShader.Set("uZoom", zoom);

        BindCommonBuffers();
        Dispatch256(edgeCount);
    }
}