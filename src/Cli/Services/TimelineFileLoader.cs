using ChangeTrace.Core.Interfaces;
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
        ISerializer<Timeline> serializer,
        string filePath,
        CancellationToken ct = default)
    {
        if (serializer is IStreamingSerializer<Timeline> streamingSerializer)
        {
            await using var stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 131072,
                options: FileOptions.Asynchronous | FileOptions.SequentialScan);

            return await streamingSerializer.DeserializeAsync(stream, ct);
        }

        var data = await File.ReadAllBytesAsync(filePath, ct);
        return await serializer.DeserializeAsync(data, ct);
    }
}
