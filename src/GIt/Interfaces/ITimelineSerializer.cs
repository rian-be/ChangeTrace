using ChangeTrace.Core;

namespace ChangeTrace.GIt.Interfaces;

/// <summary>
/// Serializes and deserializes <see cref="Timeline"/> instances to/from binary format.
/// Abstracts over the serialization format (MessagePack, etc.).
/// </summary>
internal interface ITimelineSerializer
{
    /// <summary>
    /// Serializes the given timeline into byte array.
    /// </summary>
    /// <param name="timeline">Timeline to serialize.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Byte array representing the serialized timeline.</returns>
    Task<byte[]> SerializeAsync(Timeline timeline, CancellationToken ct = default);

    /// <summary>
    /// Deserialize timeline from the given byte array.
    /// </summary>
    /// <param name="data">Serialized timeline data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The deserialized <see cref="Timeline"/> instance.</returns>
    Task<Timeline> DeserializeAsync(byte[] data, CancellationToken ct = default);
}