using ChangeTrace.Core;
using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Events.Info;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Results;
using ChangeTrace.Core.Timelines;
using ChangeTrace.GIt.Interfaces;
using Microsoft.Extensions.Logging;

namespace ChangeTrace.GIt.Enrichers;

/// <summary>
/// Base class for platform specific timeline enrichers.
/// </summary>
/// <remarks>
/// Provides logging, helper methods, and common functionality for enriching a <see cref="Timeline"/> 
/// with platform-specific data, such as pull request events from GitHub, GitLab, etc.
/// 
/// All concrete enrichers must implement <see cref="EnrichAsync"/>.
/// </remarks>

internal abstract class BasePlatformEnricher(ILogger logger) : ITimelineEnricher
{
    /// <summary>
    /// Logger instance for the enricher
    /// </summary>
    protected readonly ILogger Logger = logger;

    /// <summary>
    /// Enriches a <see cref="Timeline"/> with platform-specific events.
    /// </summary>
    /// <param name="timeline">Timeline to enrich</param>
    /// <param name="repositoryId">Repository identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An <see cref="EnrichmentResult"/> wrapped in a <see cref="Result{T}"/></returns>
    public abstract Task<Result<EnrichmentResult>> EnrichAsync(
        Timeline timeline,
        RepositoryId repositoryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Helper to attach pull request information to a trace event.
    /// </summary>
    /// <param name="traceEvent">Event to enrich</param>
    /// <param name="prNumber">Pull request number</param>
    /// <param name="prType">Pull request type (created, closed, merged)</param>
    /// <param name="metadata">Optional metadata string</param>
    protected TraceEvent EnrichTraceEventWithPr(
        in TraceEvent traceEvent,
        int prNumber,
        PullRequestEventType prType,
        string metadata)
    {
        Logger.LogDebug(
            "PR Enrichment start | Target: {Target} | Time: {Time}",
            traceEvent.Target,
            traceEvent.TimeForPlayback);

        System.Diagnostics.Debug.WriteLine(
            $"[PR ENRICH START] Target={traceEvent.Target}");

        var pr = PullRequestNumber.Create(prNumber).Value;

        var withPr = traceEvent.WithPullRequest(pr, prType);

        Logger.LogDebug(
            "PR attached | PR: {PrNumber} | Type: {Type}",
            prNumber,
            prType);

        System.Diagnostics.Debug.WriteLine(
            $"[PR ATTACHED] PR={prNumber} Type={prType}");

        var newMetadata =
            withPr.Metadata?.WithMetadata(metadata)
            ?? new MetadataInfo(metadata);

        var result = withPr.WithMetadata(newMetadata);

        Logger.LogDebug(
            "Metadata attached | Metadata: {Metadata}",
            metadata);

        System.Diagnostics.Debug.WriteLine(
            $"[METADATA ATTACHED] {metadata}");

        Logger.LogDebug(
            "PR Enrichment finished | Target: {Target}",
            result.Target);

        System.Diagnostics.Debug.WriteLine(
            $"[PR ENRICH END] Target={result.Target}");

        return result;
    }

    /// <summary>
    /// Maps GitHub pull request state and merged flag to <see cref="PullRequestEventType"/>.
    /// </summary>
    /// <param name="merged">Whether the PR was merged</param>
    /// <param name="state">PR state string from the API ("open", "closed")</param>
    /// <returns>Mapped <see cref="PullRequestEventType"/></returns>
    protected static PullRequestEventType MapPrState(bool merged, string state)
    {
        if (merged) return PullRequestEventType.PullRequestMerged;
        return state.Equals("closed", StringComparison.OrdinalIgnoreCase)
            ? PullRequestEventType.PullRequestClosed
            : PullRequestEventType.PullRequestCreated;
    }
}