using System.Numerics;
using ChangeTrace.Graphics.Gpu.Contracts;
using ChangeTrace.Rendering;
using ChangeTrace.Rendering.States;

namespace ChangeTrace.Benchmarks.Rendering;

/// <summary>
/// CPU-side data prepared in the same shape as renderer GPU buffers.
/// </summary>
/// <remarks>
/// Used only by benchmarks to measure conversion cost before OpenGL buffer upload.
/// </remarks>
internal readonly record struct GpuBufferData(
    GpuCircleNode[] Nodes,
    GpuPawn[] Pawns,
    GpuEdge[] Edges)
{
    /// <summary>
    /// Total number of GPU contract instances prepared for a frame.
    /// </summary>
    public int TotalCount =>
        Nodes.Length + Pawns.Length + Edges.Length;

    /// <summary>
    /// Builds GPU-shaped node, pawn, and edge arrays from a render state snapshot.
    /// </summary>
    /// <param name="state">Immutable render state used as conversion input.</param>
    public static GpuBufferData From(RenderState state)
    {
        var nodes = state.Scene.Nodes;
        var avatars = state.Scene.Avatars;
        var edges = state.Scene.Edges;

        var gpuNodes = new GpuCircleNode[nodes.Count];
        var gpuPawns = new GpuPawn[avatars.Count];
        var gpuEdges = new GpuEdge[edges.Count];

        for (var i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            gpuNodes[i] = new GpuCircleNode
            {
                Position = ToVector2(node.Position),
                Radius = node.Radius,
                Kind = (float)node.Kind,
                Color = node.Color,
                Glow = node.Glow
            };
        }

        for (var i = 0; i < avatars.Count; i++)
        {
            var avatar = avatars[i];
            gpuPawns[i] = new GpuPawn
            {
                Position = ToVector2(avatar.Position),
                Radius = 8.0f,
                Alpha = avatar.Alpha,
                Color = avatar.Color,
                Glow = avatar.ActivityLevel
            };
        }

        for (var i = 0; i < edges.Count; i++)
        {
            var edge = edges[i];
            var from = state.Scene.FindNode(edge.FromId);
            var to = state.Scene.FindNode(edge.ToId);

            gpuEdges[i] = new GpuEdge
            {
                From = from is null ? Vector2.Zero : ToVector2(from.Value.Position),
                To = to is null ? Vector2.Zero : ToVector2(to.Value.Position),
                Color = edge.Color,
                Alpha = edge.Alpha,
                WidthStart = edge.WidthStart,
                WidthEnd = edge.WidthEnd,
                Kind = (float)edge.Kind,
                FromGlow = from?.Glow ?? 0.0f,
                ToGlow = to?.Glow ?? 0.0f
            };
        }

        return new GpuBufferData(gpuNodes, gpuPawns, gpuEdges);
    }

    private static Vector2 ToVector2(Vec2 value)
        => new(value.X, value.Y);
}
