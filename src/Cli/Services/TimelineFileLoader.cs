using ChangeTrace.GIt.Interfaces;
using ChangeTrace.Core.Timelines;

namespace ChangeTrace.Cli.Services;

/// <summary>
/// Loads serialized timeline files without materializing the full trace as a byte array when stream support is available.
/// </summary>
internal static class TimelineFileLoader
{
    /// <summary>
    /// Loads and deserializes a timeline from disk.
    /// </summary>
    public static async Task<Timeline> LoadAsync(
        ITimelineRepository repository,
        string filePath,
        CancellationToken ct = default)
    {
        var result = await repository.LoadAsync(filePath, ct);
        if (result.IsFailure)
            throw new InvalidOperationException(result.Error ?? $"Failed to load timeline '{filePath}'.");

        return result.Value;
    }
}
