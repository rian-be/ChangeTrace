using MessagePack;
using MessagePack.Formatters;

namespace ChangeTrace.Configuration.Converters;

/// <summary>
/// MessagePack formatter for <see cref="Ulid"/> values.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Serializes <see cref="Ulid"/> as a string.</item>
/// <item>Deserializes from a string, returning <see cref="Ulid.Empty"/> if the input is null or empty.</item>
/// <item>Implements <see cref="IMessagePackFormatter{T}"/> for integration with MessagePack-CSharp.</item>
/// </list>
/// </remarks>
public sealed class UlidFormatter : IMessagePackFormatter<Ulid>
{
    /// <summary>
    /// Serializes <see cref="Ulid"/> to MessagePack string.
    /// </summary>
    /// <param name="writer">Writer to output serialized data.</param>
    /// <param name="value">Value to serialize.</param>
    /// <param name="options">Serialization options.</param>
    public void Serialize(ref MessagePackWriter writer, Ulid value, MessagePackSerializerOptions options)
    {
        writer.Write(value.ToString());
    }

    /// <summary>
    /// Deserializes <see cref="Ulid"/> from MessagePack string.
    /// </summary>
    /// <param name="reader">Reader to read serialized data.</param>
    /// <param name="options">Serialization options.</param>
    /// <returns>Parsed <see cref="Ulid"/> or <see cref="Ulid.Empty"/> if input is null or empty.</returns>
    public Ulid Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var str = reader.ReadString();
        return string.IsNullOrEmpty(str) ? Ulid.Empty : Ulid.Parse(str);
    }
}