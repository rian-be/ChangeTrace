using ChangeTrace.Core;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Results;
using ChangeTrace.GIt.Enrichers;

namespace ChangeTrace.GIt.Interfaces;

/// <summary>
/// Abstraction for timeline enrichment from external platforms (e.g., GitHub, GitLab).
/// Provides a method to augment an existing <see cref="Timeline"/> with additional events,
/// such as pull requests, issues, or other platform-specific metadata.
/// </summary>
internal interface ITimelineEnricher
{
    /// <summary>
    /// Enriches a <see cref="Timeline"/> with platform-specific events.
    /// Example: associate pull requests with commits, add issue references, tags, or metadata.
    /// </summary>
    /// <param name="timeline">Timeline to enrich. Must not be null.</param>
    /// <param name="repositoryId">Repository identifier used for platform queries.</param>
    /// <param name="cancellationToken">Optional cancellation token for async operations.</param>
    /// <returns>
    /// A <see cref="Result{EnrichmentResult}"/> indicating success or failure.
    /// Contains the number of matched events and unmatched items.
    /// </returns>
    Task<Result<EnrichmentResult>> EnrichAsync(
        Timeline timeline,
        RepositoryId repositoryId,
        CancellationToken cancellationToken = default);
}