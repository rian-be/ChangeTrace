using ChangeTrace.Core.Events;
using ChangeTrace.Core.Events.Semantic;
using ChangeTrace.Core.Interfaces;

namespace ChangeTrace.Core.Aggregators;

/// <summary>
/// Aggregates trace events with pull request metadata into semantic pull request events.
/// </summary>
internal sealed class PullRequestAggregator(SemanticEventWriter<PullRequestEvent> writer)
    : IEventAggregator<TraceEvent>
{
    /// <summary>
    /// Processes a trace event and emits a pull request event when present.
    /// </summary>
    public void Process(TraceEvent evt)
    {
        if (!evt.PullRequest.HasValue)
            return;

        var pullRequest = evt.PullRequest.Value;
        var branch = evt.Branch?.Name.Value ?? evt.Target;

        writer.Write(new PullRequestEvent(
            evt.Core.Timestamp.UnixSeconds,
            evt.Core.Actor.Value,
            branch,
            pullRequest.Number,
            pullRequest.Type,
            evt.Target));
    }

    /// <summary>
    /// Flushes the aggregator.
    /// </summary>
    public void Flush()
    {
    }
}
