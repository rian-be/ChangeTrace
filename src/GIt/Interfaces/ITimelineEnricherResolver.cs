namespace ChangeTrace.GIt.Interfaces;

/// <summary>
/// Resolves timeline enrichers by hosting provider.
/// </summary>
internal interface ITimelineEnricherResolver
{
    /// <summary>
    /// Tries to resolve an enricher for the specified provider.
    /// </summary>
    bool TryResolve(string provider, out ITimelineEnricher? enricher);
}
