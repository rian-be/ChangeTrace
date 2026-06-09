namespace ChangeTrace.GIt.Services.Sidecars;

/// <summary>
/// Repository debug snapshot shared by sidecar debug payloads.
/// </summary>
internal sealed record DebugRepositorySnapshot(string Owner, string Name);
