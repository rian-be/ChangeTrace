using ChangeTrace.Core;
using ChangeTrace.Core.Timelines;
using ChangeTrace.GIt.Interfaces;

namespace ChangeTrace.Cli.Commands.Debug;

internal static class TimelineLoader
{
    /// <summary>
    /// Loads and deserializes a timeline from a file.
    /// </summary>
    /// <param name="serializer">Timeline serializer instance.</param>
    /// <param name="filePath">Path to the MessagePack timeline file.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Deserialized timeline object, or null if a file does not exist.</returns>
    public static async Task<Timeline?> LoadAsync(ITimelineSerializer serializer, string filePath, CancellationToken ct = default)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"[red]File not found: {filePath}[/]");
            return null;
        }

        try
        {
            var data = await File.ReadAllBytesAsync(filePath, ct);
            var timeline = await serializer.DeserializeAsync(data, ct);
            return timeline;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[red]Failed to load or deserialize file '{filePath}': {ex.Message}[/]");
            return null;
        }
    }
}