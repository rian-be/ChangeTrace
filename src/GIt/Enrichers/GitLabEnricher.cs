using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Results;
using ChangeTrace.Core.Timelines;
using ChangeTrace.GIt.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChangeTrace.GIt.Enrichers;

/// <summary>
/// GitLab timeline enricher stub.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton, typeof(IProviderTimelineEnricher))]
internal sealed class GitLabEnricher(ILogger<GitLabEnricher> logger) : IProviderTimelineEnricher
{
    /// <summary>
    /// Provider handled by this enricher.
    /// </summary>
    public string Provider => "gitlab";

    /// <summary>
    /// Returns a clear not-implemented result for GitLab enrichment.
    /// </summary>
    public Task<Result<EnrichmentResult>> EnrichAsync(
        Timeline timeline,
        RepositoryId repositoryId,
        CancellationToken cancellationToken = default)
    {
        logger.LogWarning("GitLab enrichment is not implemented yet for {Repo}", repositoryId.FullName);
        return Task.FromResult(Result<EnrichmentResult>.Failure("GitLab enrichment is not implemented yet."));
    }
}
