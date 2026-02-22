using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChangeTrace.Configuration.Converters;

/// <summary>
/// JSON converter for <see cref="Ulid"/> values.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Serializes <see cref="Ulid"/> as string.</item>
/// <item>Deserializes from a string, returning <see cref="Ulid.Empty"/> if the input is null or empty.</item>
/// <item>Integrates with <see cref="System.Text.Json"/> serializer.</item>
/// </list>
/// </remarks>
internal class UlidJsonConverter : JsonConverter<Ulid>
{
    /// <inheritdoc />
    public override Ulid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var stringValue = reader.GetString();
        return string.IsNullOrEmpty(stringValue) ? Ulid.Empty : Ulid.Parse(stringValue);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Ulid value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}