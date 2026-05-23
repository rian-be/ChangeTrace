using ChangeTrace.Core.Aggregators;
using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Events.Semantic;
using ChangeTrace.Core.Interfaces;
using ChangeTrace.Core.Timelines;

namespace ChangeTrace.Cli.Handlers.Debug.Player;

/// <summary>
/// Aggregates timeline events for player debug diagnostics.
/// </summary>
internal sealed class PlayerDebugAggregation
{
    private PlayerDebugAggregation(
        Dictionary<string, CommitBundleEvent> commitsBySha,
        Dictionary<(string Branch, BranchEventType Type), BranchEvent> branchesByKey,
        MergeEvent[] merges,
        int commitBundleCount,
        int branchCount,
        int mergeCount)
    {
        CommitsBySha = commitsBySha;
        BranchesByKey = branchesByKey;
        Merges = merges;
        CommitBundleCount = commitBundleCount;
        BranchCount = branchCount;
        MergeCount = mergeCount;
    }

    /// <summary>
    /// Commit bundle events indexed by commit SHA.
    /// </summary>
    public Dictionary<string, CommitBundleEvent> CommitsBySha { get; }

    /// <summary>
    /// Branch events indexed by branch name and event type.
    /// </summary>
    public Dictionary<(string Branch, BranchEventType Type), BranchEvent> BranchesByKey { get; }

    /// <summary>
    /// Aggregated merge events.
    /// </summary>
    public MergeEvent[] Merges { get; }

    private int CommitBundleCount { get; }

    private int BranchCount { get; }

    private int MergeCount { get; }

    /// <summary>
    /// Builds debug aggregation state from a timeline.
    /// </summary>
    public static PlayerDebugAggregation Build(Timeline timeline)
    {
        var commitWriter = new SemanticEventWriter<CommitBundleEvent>(1000);
        var branchWriter = new SemanticEventWriter<BranchEvent>(1000);
        var mergeWriter = new SemanticEventWriter<MergeEvent>(1000);

        var aggregators = new IEventAggregator<TraceEvent>[]
        {
            new CommitBundlingAggregator(commitWriter),
            new BranchAggregator(branchWriter),
            new MergeAggregator(mergeWriter)
        };

        using (var engine = new EventAggregationEngine<TraceEvent>(aggregators))
        {
            engine.Process(timeline.EventsSpan);
        }

        var commitsBySha = new Dictionary<string, CommitBundleEvent>();

        foreach (var commit in commitWriter.Snapshot().Span)
            commitsBySha[commit.CommitSha] = commit;

        var branchesByKey =
            new Dictionary<(string Branch, BranchEventType Type), BranchEvent>();

        foreach (var branch in branchWriter.Snapshot().Span)
        {
            var key = (branch.Branch, branch.Type);
            branchesByKey.TryAdd(key, branch);
        }

        return new PlayerDebugAggregation(
            commitsBySha,
            branchesByKey,
            mergeWriter.Snapshot().Span.ToArray(),
            commitWriter.Count,
            branchWriter.Count,
            mergeWriter.Count);
    }

    /// <summary>
    /// Prints aggregation counters to the console.
    /// </summary>
    public void PrintSummary()
    {
        Console.WriteLine("=== Aggregated Events ===");
        Console.WriteLine($"CommitBundles: {CommitBundleCount}");
        Console.WriteLine($"Branches: {BranchCount}");
        Console.WriteLine($"Merges: {MergeCount}\n");
    }
}