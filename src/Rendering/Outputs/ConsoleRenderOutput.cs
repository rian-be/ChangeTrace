using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.States;

namespace ChangeTrace.Rendering.Outputs;

/// <summary>
/// Debug implementation of <see cref="IRenderOutput"/> that writes detailed scene and HUD info to console.
/// </summary>
/// <remarks>
/// Prints summary every 10 frames, including:
/// <list type="bullet">
/// <item>Frame number, virtual time, playback speed, HUD progress</item>
/// <item>Scene statistics (nodes, glowing nodes, avatars, active avatars, edges, particles, events)</item>
/// <item>Camera position and zoom</item>
/// <item>Top 3 actors on the leaderboard</item>
/// <item>Active avatars with position, alpha, and activity level</item>
/// <item>Scene node bounds</item>
/// </list>
/// Useful for development, testing, or headless debugging without graphical output.
/// </remarks>
internal sealed class ConsoleRenderOutput : IRenderOutput
{
    private int _frameCount;

    public void Submit(RenderState state)
    {
        _frameCount++;
        if (_frameCount % 10 != 0) return;

        var scene = state.Scene;
        var hud   = state.Hud;
        var stats = scene.ComputeStats();

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine(
            $"┌─ Frame #{_frameCount,5} │ t={state.VirtualTime:F2}s │ " +
            $"{hud.SpeedLabel,-8} │ {hud.TimeLabel} │ {hud.Progress:P0}");

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(
            $"│  nodes={stats.NodeCount}({stats.GlowingNodes}✦) " +
            $"avatars={stats.AvatarCount}({stats.ActiveAvatars}⚡) " +
            $"edges={stats.VisibleEdges} " +
            $"particles={stats.ParticleCount} " +
            $"events={hud.EventsFired}/{hud.TotalEvents}");

        // Camera
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"│  cam={state.Camera.Position} zoom={state.Camera.Zoom:F2}×");

        // Leaderboard
        foreach (var (entry, i) in hud.Leaderboard.Take(3).Select((e, i) => (e, i)))
        {
            Console.ForegroundColor = i == 0 ? ConsoleColor.Yellow : ConsoleColor.Gray;
            Console.WriteLine($"│  #{i + 1} {entry.Actor,-20} {entry.EventCount} commits");
        }

        // Active avatars via SceneSnapshot query
        foreach (var avatar in scene.ActiveAvatars())
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"│  ● {avatar.Actor,-20} pos={avatar.Position} α={avatar.Alpha:F2} act={avatar.ActivityLevel:F2}");
        }

        // Bounds
        if (scene.NodeBounds() is { } bounds)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"│  bounds={bounds.Min}→{bounds.Max} size={bounds.Width:F0}×{bounds.Height:F0}");
        }

        Console.ResetColor();
        Console.WriteLine("└" + new string('─', 70));
    }
}
