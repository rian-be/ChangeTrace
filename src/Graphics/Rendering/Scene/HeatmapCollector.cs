using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.States;
using OpenTK.Mathematics;

namespace ChangeTrace.Graphics.Rendering.Scene;

/// <summary>
/// Collects active scene nodes into a heatmap influence object.
/// </summary>
internal sealed class HeatmapCollector
{
    /// <summary>
    /// Minimum node glow required to contribute to the heatmap.
    /// </summary>
    private const float MinHeatmapGlow = 0.03f;

    private Vector4[] _buffer = [];

    /// <summary>
    /// Builds heatmap input objects for the current frame.
    /// </summary>
    public ReadOnlySpan<Vector4> Collect(
        RenderState state,
        Matrix3 viewProjection,
        Vector2 heatmapSize,
        int maxObjects)
    {
        EnsureCapacity(
            maxObjects);

        int count = 0;

        foreach (var node in state.Scene.Nodes)
        {
            if (count >= maxObjects)
                break;

            if (node.Glow < MinHeatmapGlow)
                continue;

            Vector2 ndc = ProjectToNdc(
                    viewProjection,
                    node.Position.X,
                    node.Position.Y);

            if (!IsInsideHeatmapBounds(
                    ndc))
            {
                continue;
            }

            Vector2 texturePosition = ToTexturePosition(ndc, heatmapSize);

            float radius = GetRadius(node.Kind);

            float intensity = MathF.Min(
                    2.2f,
                    0.65f + node.Glow * 1.1f);

            _buffer[count++] =
                new Vector4(
                    texturePosition.X,
                    texturePosition.Y,
                    radius,
                    intensity);
        }

        return _buffer.AsSpan(
            0,
            count);
    }

    /// <summary>
    /// Ensures the reusable collection buffer can hold the requested count.
    /// </summary>
    private void EnsureCapacity(
        int count)
    {
        if (_buffer.Length >= count)
            return;

        _buffer =
            new Vector4[count];
    }

    /// <summary>
    /// Projects a world-space point into normalized device coordinates.
    /// </summary>
    private static Vector2 ProjectToNdc(
        Matrix3 viewProjection,
        float x,
        float y)
    {
        float ndcX =
            viewProjection.M11 * x +
            viewProjection.M21 * y +
            viewProjection.M31;

        float ndcY =
            viewProjection.M12 * x +
            viewProjection.M22 * y +
            viewProjection.M32;

        return new Vector2(
            ndcX,
            ndcY);
    }

    /// <summary>
    /// Checks whether a projected point is near enough to affect the heatmap.
    /// </summary>
    private static bool IsInsideHeatmapBounds(
        Vector2 ndc) =>
            ndc.X is >= -1.25f and <= 1.25f &&
            ndc.Y is >= -1.25f and <= 1.25f;

    /// <summary>
    /// Converts normalized device coordinates into heatmap texture coordinates.
    /// </summary>
    private static Vector2 ToTexturePosition(Vector2 ndc, Vector2 heatmapSize) => new(
            (ndc.X + 1.0f) * 0.5f * heatmapSize.X,
            (ndc.Y + 1.0f) * 0.5f * heatmapSize.Y);

    /// <summary>
    /// Resolves heatmap radius by node kind.
    /// </summary>
    private static float GetRadius(NodeKind kind) =>
        kind switch
        {
            NodeKind.Root => 8.0f,
            NodeKind.Branch => 6.5f,
            NodeKind.File => 4.5f,
            _ => 5.0f
        };
}