namespace ChangeTrace.Core.Interfaces;

/// <summary>
/// Optional serializer extension for stream-based serialization and deserialization.
/// </summary>
internal interface IStreamingSerializer<T>
{
    /// <summary>
    /// Serializes the given object directly into the destination stream.
    /// </summary>
    Task SerializeAsync(Stream destination, T obj, CancellationToken ct = default);

    /// <summary>
    /// Deserializes an object directly from the source stream.
    /// </summary>
    Task<T> DeserializeAsync(Stream source, CancellationToken ct = default);
}
