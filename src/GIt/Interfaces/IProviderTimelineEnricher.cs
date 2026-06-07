namespace ChangeTrace.GIt.Interfaces;

/// <summary>
/// Timeline enricher bound to a specific hosting provider.
/// </summary>
internal interface IProviderTimelineEnricher : ITimelineEnricher
{
    /// <summary>
    /// Gets the provider identifier handled by this enricher.
    /// </summary>
    string Provider { get; }
}
