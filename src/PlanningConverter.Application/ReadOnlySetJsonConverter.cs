using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlanningConverter.Application;

/// <summary>
/// System.Text.Json ne sait pas relire <c>IReadOnlySet&lt;T&gt;</c> : sans ce convertisseur,
/// une sauvegarde automatique ne pouvait pas être rouverte (« Reprendre la dernière conversion »).
/// </summary>
public sealed class ReadOnlySetJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(IReadOnlySet<>);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var element = typeToConvert.GetGenericArguments()[0];
        return (JsonConverter)Activator.CreateInstance(typeof(Converter<>).MakeGenericType(element))!;
    }

    private sealed class Converter<T> : JsonConverter<IReadOnlySet<T>>
    {
        public override IReadOnlySet<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var items = JsonSerializer.Deserialize<List<T>>(ref reader, options);
            return items is null ? null : new HashSet<T>(items);
        }

        public override void Write(Utf8JsonWriter writer, IReadOnlySet<T> value, JsonSerializerOptions options) =>
            JsonSerializer.Serialize(writer, value.ToList(), options);
    }
}
