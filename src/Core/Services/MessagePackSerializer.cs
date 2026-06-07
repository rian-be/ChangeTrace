using ChangeTrace.Configuration.Converters;
using ChangeTrace.Configuration.Discovery;
using MessagePack;
using MessagePack.Formatters;
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
internal sealed class MessagePackSerializer<T> : ISerializer<T>, IStreamingSerializer<T>
{
    private readonly MessagePackSerializerOptions _options;

    /// <summary>
    /// Initializes serializer with <see cref="UlidFormatter"/> and standard resolvers.
    /// </summary>
    public MessagePackSerializer(IEnumerable<IMessagePackFormatter<T>> formatters)
    {
        var resolver = CompositeResolver.Create(
            formatters.Cast<IMessagePackFormatter>().Append(new UlidFormatter()).ToArray(),
            [StandardResolverAllowPrivate.Instance]
        );

        _options = MessagePackSerializerOptions.Standard
            .WithCompression(MessagePackCompression.Lz4BlockArray)
            .WithResolver(resolver);
    }

    /// <summary>
    /// Serializes an object to a byte array asynchronously.
    /// </summary>
    /// <param name="obj">Object to serialize.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Byte array representing serialized object.</returns>
    public Task<byte[]> SerializeAsync(T obj, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(MessagePackSerializer.Serialize(obj, _options));
    }

    /// <summary>
    /// Deserializes an object from a byte array asynchronously.
    /// </summary>
    /// <param name="data">Byte array to deserialize.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Deserialized object of type <typeparamref name="T"/>.</returns>
    public Task<T> DeserializeAsync(byte[] data, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(MessagePackSerializer.Deserialize<T>(data, _options));
    }

    /// <summary>
    /// Serializes an object directly into a stream.
    /// </summary>
    public async Task SerializeAsync(Stream destination, T obj, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        await MessagePackSerializer.SerializeAsync(destination, obj, _options, ct);
    }

    /// <summary>
    /// Deserializes an object directly from a stream.
    /// </summary>
    public async Task<T> DeserializeAsync(Stream source, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return await MessagePackSerializer.DeserializeAsync<T>(source, _options, ct);
    }
}
