using ChangeTrace.Player.Interfaces;

namespace ChangeTrace.Cli.Handlers.Debug.Player;

/// <summary>
/// Prints timeline player diagnostics to the console.
/// </summary>
internal static class PlayerDebugStatePrinter
{
    /// <summary>
    /// Writes current player state and playback statistics.
    /// </summary>
    public static void Print(ITimelinePlayer player)
    {
        var diag = player.GetDiagnostics();

        Console.WriteLine($"State: {diag.State}, Mode: {diag.Mode}, Direction: {diag.Direction}");
        Console.WriteLine($"Position: {diag.PositionSeconds:F2}/{diag.DurationSeconds:F2} ({diag.Progress:P1})");
        Console.WriteLine($"Speed: {diag.CurrentSpeed:F2}/{diag.TargetSpeed:F2}");
        Console.WriteLine($"Events: {diag.EventsFired}/{diag.TotalEvents}, Loops: {diag.LoopCount}");
    }
}