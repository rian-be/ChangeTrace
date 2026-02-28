using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.States;

namespace ChangeTrace.Rendering.Outputs;

/// <summary>
/// Debug implementation of <see cref="IRenderOutput"/> that prints scene snapshots to console.
/// </summary>
/// <remarks>
/// Useful for development or testing without a graphical UI.
/// Logs all nodes, active avatars, and basic scene statistics.
/// </remarks>
internal sealed class DebugRenderOutput : IRenderOutput
{
    /// <summary>
    /// Submits <see cref="RenderState"/> by writing nodes, avatars, and stats to console.
    /// </summary>
    /// <param name="state">The snapshot of scene to output.</param>
    public void Submit(RenderState state)
    {
        var scene = state.Scene;
        
        foreach (var node in scene.Nodes)
        {
            Console.WriteLine(
                $"[Node] '{node.Id}' kind={node.Kind} pos={node.Position} color={node.Color:X6} glow={node.Glow:F2}");
        }
        
        foreach (var avatar in scene.ActiveAvatars())
        {
            Console.WriteLine(
                $"[Avatar] '{avatar.Actor}' pos={avatar.Position} α={avatar.Alpha:F2} act={avatar.ActivityLevel:F2}");
        }
        
        var stats = scene.ComputeStats();
        Console.WriteLine(
            $"Scene stats: nodes={stats.NodeCount}, glowing={stats.GlowingNodes}, " +
            $"avatars={stats.AvatarCount}, active={stats.ActiveAvatars}, edges={stats.VisibleEdges}");
    }
}