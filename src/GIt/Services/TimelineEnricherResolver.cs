using ChangeTrace.Configuration.Discovery;
using ChangeTrace.GIt.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.GIt.Services;

/// <summary>
/// Provider enricher lookup.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton, typeof(ITimelineEnricherResolver))]
internal sealed class TimelineEnricherResolver(IEnumerable<IProviderTimelineEnricher> enrichers) : ITimelineEnricherResolver
{
    private readonly IReadOnlyDictionary<string, ITimelineEnricher> _enrichers =
        enrichers
            .Where(enricher => !string.IsNullOrWhiteSpace(enricher.Provider))
            .GroupBy(enricher => enricher.Provider, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => (ITimelineEnricher)group.First(), StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Tries to resolve an enricher for the specified provider.
    /// </summary>
    public bool TryResolve(string provider, out ITimelineEnricher? enricher)
        => _enrichers.TryGetValue(provider, out enricher);
}
