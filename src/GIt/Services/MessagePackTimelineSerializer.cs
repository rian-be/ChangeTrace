using ChangeTrace.Configuration;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core;
using ChangeTrace.Core.Timelines;
using ChangeTrace.GIt.Dto;
using ChangeTrace.GIt.Interfaces;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.GIt.Services;

/// <summary>
/// Serializer for <see cref="Timeline"/> using MessagePack format.
/// Efficient, compact, supports LZ4 compression.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class MessagePackTimelineSerializer : ITimelineSerializer
{
    private static readonly MessagePackSerializerOptions Options = MessagePackSerializerOptions.Standard
        .WithCompression(MessagePackCompression.Lz4BlockArray)
        .WithResolver(StandardResolverAllowPrivate.Instance);

    /// <summary>
    /// Serializes timeline into MessagePack byte array.
    /// </summary>
    /// <param name="timeline">Timeline to serialize.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Byte array representing the serialized timeline.</returns>
    public Task<byte[]> SerializeAsync(Timeline timeline, CancellationToken cancellationToken = default)
    {
        var dto = TimelineDto.FromDomain(timeline);
        var bytes = MessagePackSerializer.Serialize(dto, Options, cancellationToken);
        return Task.FromResult(bytes);
    }

    /// <summary>
    /// Deserializes MessagePack byte array into <see cref="Timeline"/>.
    /// </summary>
    /// <param name="data">Byte array containing serialized timeline.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Deserialized <see cref="Timeline"/> instance.</returns>
    public Task<Timeline> DeserializeAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        try
        {
            Console.WriteLine($"[MessagePackSerializer] Deserializing {data.Length} bytes...");
        
            var dto = MessagePackSerializer.Deserialize<TimelineDto>(data, Options, cancellationToken);
        
            if (dto == null)
            {
                Console.WriteLine("[MessagePackSerializer] ERROR: DTO is null after deserialization!");
                throw new InvalidOperationException("Deserialized DTO is null");
            }
        
            Console.WriteLine($"[MessagePackSerializer] DTO deserialized successfully");
            Console.WriteLine($"[MessagePackSerializer] Converting to domain model...");
        
            var timeline = dto.ToDomain();
        
            if (timeline == null)
            {
                Console.WriteLine("[MessagePackSerializer] ERROR: ToDomain() returned null!");
                throw new InvalidOperationException("ToDomain() returned null");
            }
        
            Console.WriteLine($"[MessagePackSerializer] Timeline created with {timeline.Events?.Count ?? 0} events");
            return Task.FromResult(timeline);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MessagePackSerializer] EXCEPTION during deserialization:");
            Console.WriteLine($"  Type: {ex.GetType().Name}");
            Console.WriteLine($"  Message: {ex.Message}");
            Console.WriteLine($"  Stack:\n{ex.StackTrace}");
            throw;
        }
    }
}