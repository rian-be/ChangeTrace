namespace ChangeTrace.GIt.Options;

/// <summary>
/// Enrichment scopes available during export.
/// </summary>
[Flags]
internal enum ExportEnrichmentKind
{
    None = 0,
    PullRequests = 1
}
