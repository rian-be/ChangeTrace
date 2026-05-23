using ChangeTrace.Core.Events.Semantic;

namespace ChangeTrace.Cli.Handlers.Debug.Player;

/// <summary>
/// Renders aggregated semantic events to the debug console.
/// </summary>
internal sealed class PlayerDebugRenderer
{
    private int _renderCount;

    /// <summary>
    /// Total number of rendered debug events.
    /// </summary>
    public int RenderCount => _renderCount;

    /// <summary>
    /// Renders summarized commit bundle information.
    /// </summary>
    public void RenderCommit(CommitBundleEvent bundle)
    {
        _renderCount++;

        Console.WriteLine(
            $"\n[{_renderCount}] COMMIT {bundle.CommitSha[..7]} by {bundle.Actor}");

        var toShow = Math.Min(3, bundle.Files.Length);

        for (var i = 0; i < toShow; i++)
            Console.WriteLine($"      + {bundle.Files.Span[i]}");

        if (bundle.Files.Length > 3)
        {
            Console.WriteLine(
                $"      ... and {bundle.Files.Length - 3} more");
        }
    }

    /// <summary>
    /// Renders branch lifecycle events.
    /// </summary>
    public void RenderBranch(BranchEvent branch)
    {
        _renderCount++;

        Console.WriteLine(
            $"[{_renderCount}] BRANCH {branch.Branch} → {branch.Type} by {branch.Actor}");
    }

    /// <summary>
    /// Renders merge event diagnostics.
    /// </summary>
    public void RenderMerge(MergeEvent merge)
    {
        _renderCount++;

        Console.WriteLine(
            $"[{_renderCount}] MERGE {merge.SourceBranch} → {merge.TargetBranch} by {merge.Actor}");

        Console.WriteLine(
            $"    Files merged: {merge.FilesMerged.Length}");
    }
}