using ChangeTrace.Configuration.Converters;
using ChangeTrace.Configuration.Discovery;
using MessagePack;
using MessagePack.Resolvers;
using ChangeTrace.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Core.Services;

/// <summary>
/// Generic MessagePack serializer using custom <see cref="UlidFormatter"/> and standard resolvers.
/// </summary>
/// <typeparam name="T">Type to serialize/deserialize.</typeparam>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="ISerializer{T}"/> for async serialization and deserialization.</item>
/// <item>Supports <see cref="Ulid"/> via <see cref="UlidFormatter"/>.</item>
/// <item>Uses <see cref="CompositeResolver"/> to combine custom and standard resolvers.</item>
/// <item>Registered as singleton via <see cref="AutoRegisterAttribute"/>.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class MessagePackSerializer<T> : ISerializer<T>
{
    private readonly MessagePackSerializerOptions _options;

    /// <summary>
    /// Initializes serializer with <see cref="UlidFormatter"/> and standard resolvers.
    /// </summary>
    public MessagePackSerializer()
    {
        var resolver = CompositeResolver.Create(
            [new UlidFormatter()],
            [StandardResolver.Instance]
        );

        _options = MessagePackSerializerOptions.Standard.WithResolver(resolver);
    }

    /// <summary>
    /// Serializes an object to a byte array asynchronously.
    /// </summary>
    /// <param name="obj">Object to serialize.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Byte array representing serialized object.</returns>
    public async Task<byte[]> SerializeAsync(T obj, CancellationToken ct = default)
    {
        await using var ms = new MemoryStream();
        await MessagePackSerializer.SerializeAsync(ms, obj, _options, ct);
        return ms.ToArray();
    }

    /// <summary>
    /// Deserializes an object from a byte array asynchronously.
    /// </summary>
    /// <param name="data">Byte array to deserialize.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Deserialized object of type <typeparamref name="T"/>.</returns>
    public async Task<T> DeserializeAsync(byte[] data, CancellationToken ct = default)
    {
        await using var ms = new MemoryStream(data);
        return await MessagePackSerializer.DeserializeAsync<T>(ms, _options, ct);
    }
}