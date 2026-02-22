namespace ChangeTrace.Core.Interfaces;

/// <summary>
/// Generic serializer interface for any type.
/// </summary>
internal interface ISerializer<T>
{
    /// <summary>
    /// Serializes the given object into a byte array.
    /// </summary>
    /// <param name="obj">Object to serialize.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Byte array representing the serialized object.</returns>
    Task<byte[]> SerializeAsync(T obj, CancellationToken ct = default);

    /// <summary>
    /// Deserializes object from the given byte array.
    /// </summary>
    /// <param name="data">Serialized data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Deserialized instance of <typeparamref name="T"/>.</returns>
    Task<T> DeserializeAsync(byte[] data, CancellationToken ct = default);
}