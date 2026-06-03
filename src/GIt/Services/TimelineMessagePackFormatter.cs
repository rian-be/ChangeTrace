using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Timelines;
using ChangeTrace.GIt.Dto;
using MessagePack;
using MessagePack.Formatters;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.GIt.Services;

/// <summary>
/// MessagePack formatter that maps <see cref="Timeline"/> through its persistence DTO.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton, typeof(IMessagePackFormatter<Timeline>))]
internal sealed class TimelineMessagePackFormatter : IMessagePackFormatter<Timeline>
{
    public void Serialize(ref MessagePackWriter writer, Timeline value, MessagePackSerializerOptions options)
    {
        var dto = TimelineDto.FromDomain(value);
        MessagePackSerializer.Serialize(ref writer, dto, options);
    }

    public Timeline Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var dto = MessagePackSerializer.Deserialize<TimelineDto>(ref reader, options);

        return dto?.ToDomain()
            ?? throw new InvalidOperationException("Deserialized timeline DTO is null.");
    }
}
