using ChangeTrace.Rendering.Scene;

namespace ChangeTrace.Rendering.Layout.Hive.Core;

/// <summary>
/// Smoothly moves nodes toward their computed hive layout targets.
/// </summary>
internal sealed class HiveNodeAnimator(HiveLayoutOptions options)
{
    /// <summary>
    /// Interpolates node positions toward their home positions and returns total layout energy.
    /// </summary>
    public float AnimateToHome(
        IReadOnlyDictionary<string, SceneNode> nodes)
    {
        var energy = 0f;

        foreach (var node in nodes.Values)
        {
            if (node.Pinned)
                continue;

            var delta = node.HomePosition - node.Position;

            energy += delta.LengthSq;

            if (delta.LengthSq < 0.25f)
            {
                node.Position = node.HomePosition;
                node.Velocity = Vec2.Zero;
                node.Force = Vec2.Zero;
                continue;
            }

            node.Position = Vec2.Lerp(
                node.Position,
                node.HomePosition,
                options.AnimationSmoothness);

            node.Velocity = Vec2.Zero;
            node.Force = Vec2.Zero;
        }

        return energy;
    }
}