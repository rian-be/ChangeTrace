using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Timelines;
using ChangeTrace.Player.Enums;
using ChangeTrace.Player.Factory;
using ChangeTrace.Player.Interfaces;

namespace ChangeTrace.Cli.Handlers.Debug.Player;

/// <summary>
/// Runs timeline playback and renders matching debug events.
/// </summary>
internal sealed class PlayerDebugSession(
    ITimelinePlayerFactory playerFactory,
    Timeline timeline,
    PlayerDebugAggregation aggregation,
    PlayerDebugRenderer renderer)
{
    private readonly HashSet<string> _processedCommits = [];
    private readonly HashSet<(string Branch, BranchEventType Type)> _processedBranches = [];
    private readonly HashSet<(string SourceBranch, string TargetBranch)> _processedMerges = [];

    /// <summary>
    /// Timeline player used by the debug session.
    /// </summary>
    public ITimelinePlayer Player { get; } = playerFactory.Create(
        timeline,
        initialSpeed: 2.0,
        acceleration: 1.0);

    /// <summary>
    /// Starts playback and renders semantic debug output.
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        Player.OnEvent += OnEvent;

        try
        {
            Console.WriteLine("=== Starting Playback ===\n");

            var playResult = Player.Play();

            if (!playResult.IsSuccess)
            {
                Console.WriteLine($"Play failed: {playResult.Error}");
                return;
            }

            while (Player.State == PlayerState.Playing &&
                   !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(100, cancellationToken);
            }

            Player.Stop();
        }
        finally
        {
            Player.OnEvent -= OnEvent;
        }
    }

    /// <summary>
    /// Handles raw playback events and renders matching semantic events.
    /// </summary>
    private void OnEvent(TraceEvent traceEvent)
    {
        TryRenderCommit(traceEvent);
        TryRenderBranch(traceEvent);
        TryRenderMerge(traceEvent);
    }

    /// <summary>
    /// Renders a commit bundle once for the matching commit event.
    /// </summary>
    private void TryRenderCommit(TraceEvent traceEvent)
    {
        if (traceEvent.Commit?.Sha.Value == null)
            return;

        var sha = traceEvent.Commit.Value.Sha;

        if (!aggregation.CommitsBySha.TryGetValue(sha, out var bundle))
            return;

        if (!_processedCommits.Add(sha))
            return;

        renderer.RenderCommit(bundle);
    }

    /// <summary>
    /// Renders a branch event once for the matching branch state.
    /// </summary>
    private void TryRenderBranch(TraceEvent traceEvent)
    {
        if (traceEvent.Branch == null)
            return;

        var key = (
            traceEvent.Branch.Value.Name.Value,
            traceEvent.Branch.Value.Type);

        if (!aggregation.BranchesByKey.TryGetValue(key, out var branch))
            return;

        if (!_processedBranches.Add(key))
            return;

        renderer.RenderBranch(branch);
    }

    /// <summary>
    /// Renders the nearest unprocessed merge event for the playback timestamp.
    /// </summary>
    private void TryRenderMerge(TraceEvent traceEvent)
    {
        if (aggregation.Merges.Length == 0)
            return;

        var timestamp = traceEvent.TimeForPlayback;

        var merge = aggregation.Merges
            .SkipWhile(x => _processedMerges.Contains((x.SourceBranch, x.TargetBranch)))
            .OrderBy(x => Math.Abs(x.Timestamp - timestamp))
            .FirstOrDefault();

        if (merge.Timestamp <= 0)
            return;

        var key = (merge.SourceBranch, merge.TargetBranch);

        if (!_processedMerges.Add(key))
            return;

        renderer.RenderMerge(merge);
    }
}