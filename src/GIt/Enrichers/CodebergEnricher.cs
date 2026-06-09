using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Results;
using ChangeTrace.Core.Timelines;
using ChangeTrace.GIt.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChangeTrace.GIt.Enrichers;

/// <summary>
/// Codeberg timeline enricher stub.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton, typeof(IProviderTimelineEnricher))]
internal sealed class CodebergEnricher(ILogger<CodebergEnricher> logger) : IProviderTimelineEnricher
{
    /// <summary>
    /// Provider handled by this enricher.
    /// </summary>
    public string Provider => "codeberg";

    /// <summary>
    /// Returns a clear not-implemented result for Codeberg enrichment.
    /// </summary>
    public Task<Result<EnrichmentResult>> EnrichAsync(
        Timeline timeline,
        RepositoryId repositoryId,
        CancellationToken cancellationToken = default)
    {
        logger.LogWarning("Codeberg enrichment is not implemented yet for {Repo}", repositoryId.FullName);
        return Task.FromResult(Result<EnrichmentResult>.Failure("Codeberg enrichment is not implemented yet."));
    }
}
