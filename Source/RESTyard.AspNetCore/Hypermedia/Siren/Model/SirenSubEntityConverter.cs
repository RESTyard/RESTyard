using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RESTyard.AspNetCore.Hypermedia.Siren.Model;

/// <summary>
/// Custom JSON converter for <see cref="SirenSubEntity"/> that serializes derived type properties
/// based on the runtime type. This avoids the need for <see cref="JsonDerivedTypeAttribute"/>
/// which cannot register open generic types like <see cref="SirenEmbeddedEntity{TProperties}"/>.
/// <para>
/// Deserialization uses structural discrimination: if the JSON contains <c>properties</c> or <c>entities</c>,
/// it deserializes as <see cref="SirenEmbeddedEntity{TProperties}"/> (with <see cref="JsonElement"/> as TProperties).
/// If it contains <c>href</c>, it deserializes as <see cref="SirenLinkedEntity"/>.
/// </para>
/// </summary>
public class SirenSubEntityConverter : JsonConverter<SirenSubEntity>
{
    /// <summary>
    /// Only handle the abstract base type. Derived types (<see cref="SirenEmbeddedEntity{TProperties}"/>,
    /// <see cref="SirenLinkedEntity"/>) are deserialized by the default object deserializer,
    /// preventing recursion when <see cref="Read"/> calls <c>Deserialize</c> for a concrete type.
    /// </summary>
    public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(SirenSubEntity);

    public override SirenSubEntity? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // get the json doc so we can decide the type
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var root = jsonDoc.RootElement;

        // determine type
        // Rule: If it has 'properties' or 'entities', it is an Embedded Entity.
        // Rule: If it has 'href' and no 'properties', it is a Linked Entity.
        // Use the JSON property names (matching [JsonPropertyName] values), not C# property names
        var hasProperties = root.TryGetProperty("properties", out _);
        var hasEntities = root.TryGetProperty("entities", out _);
        var hasHref = root.TryGetProperty("href", out _);

        if (hasProperties || hasEntities)
        {
            // It's an Embedded Entity. 
            // We use JsonElement as the generic type so we don't lose data 
            // regardless of what the 'properties' object contains.
            return root.Deserialize<SirenEmbeddedEntity<JsonElement>>(options);
        }
        
        if (hasHref)
        {
            // It's a Linked Entity.
            return root.Deserialize<SirenLinkedEntity>(options);
        }

        // Fallback for empty/minimal sub-entities
        throw new JsonException("Siren SubEntity must have either 'href' or 'properties/entities'.");
    }

    public override void Write(Utf8JsonWriter writer, SirenSubEntity value, JsonSerializerOptions options)
    {
        // Serialize using the runtime type so all derived properties are included.
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}
