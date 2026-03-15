using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;

namespace RESTyard.Schema;

/// <summary>
/// Extension methods for extracting common keywords from <see cref="JsonSchema"/>.
/// Used internally by mappers — public API uses <see cref="JsonDocument"/>.
/// </summary>
internal static class JsonSchemaExtensions
{
    /// <summary>
    /// Parses a <see cref="JsonDocument"/> into a <see cref="JsonSchema"/>.
    /// </summary>
    internal static JsonSchema ToJsonSchema(this JsonDocument document)
        => JsonSchema.FromText(document.RootElement.GetRawText());

    internal static IReadOnlyDictionary<string, JsonSchema>? GetProperties(this JsonSchema schema)
        => schema.Keywords?.OfType<PropertiesKeyword>().FirstOrDefault()?.Properties;

    internal static SchemaValueType? GetSchemaType(this JsonSchema schema)
        => schema.Keywords?.OfType<TypeKeyword>().FirstOrDefault()?.Type;

    internal static IReadOnlyList<string>? GetRequired(this JsonSchema schema)
        => schema.Keywords?.OfType<RequiredKeyword>().FirstOrDefault()?.Properties;

    internal static string? GetDescription(this JsonSchema schema)
        => schema.Keywords?.OfType<DescriptionKeyword>().FirstOrDefault()?.Value;

    internal static JsonSchema? GetItemsSchema(this JsonSchema schema)
        => schema.Keywords?.OfType<ItemsKeyword>().FirstOrDefault()?.SingleSchema;

    internal static Uri? GetRef(this JsonSchema schema)
        => schema.Keywords?.OfType<RefKeyword>().FirstOrDefault()?.Reference;

    internal static string? GetFormat(this JsonSchema schema)
        => schema.Keywords?.OfType<FormatKeyword>().FirstOrDefault()?.Value.Key;

    internal static IReadOnlyList<JsonNode?>? GetEnum(this JsonSchema schema)
        => schema.Keywords?.OfType<EnumKeyword>().FirstOrDefault()?.Values;

    internal static JsonNode? GetDefault(this JsonSchema schema)
        => schema.Keywords?.OfType<DefaultKeyword>().FirstOrDefault()?.Value;

    /// <summary>
    /// Produces a human-readable type string for a property schema.
    /// Resolves array item types and <c>$ref</c> to definition names.
    /// </summary>
    internal static string SchemaToTypeString(JsonSchema propSchema)
        => SchemaToTypeString(propSchema, linkDefinitions: false);

    /// <summary>
    /// Like <see cref="SchemaToTypeString(JsonSchema)"/> but wraps <c>$ref</c> definition names
    /// in Markdown links (e.g., <c>[Address](#definition-address)</c>).
    /// </summary>
    internal static string SchemaToLinkedTypeString(JsonSchema propSchema)
        => SchemaToTypeString(propSchema, linkDefinitions: true);

    private static string SchemaToTypeString(JsonSchema propSchema, bool linkDefinitions)
    {
        // $ref → extract definition name from path (e.g., "#/definitions/Address" → "Address")
        var refUri = propSchema.GetRef();
        if (refUri != null)
        {
            var refString = refUri.OriginalString;
            var lastSlash = refString.LastIndexOf('/');
            if (lastSlash >= 0 && lastSlash < refString.Length - 1)
            {
                var name = refString.Substring(lastSlash + 1);
                return linkDefinitions
                    ? $"[{name}](#definition-{name.ToLowerInvariant()})"
                    : name;
            }
            return "object";
        }

        var type = propSchema.GetSchemaType();
        if (type == null)
            return "object";

        // Handle pure Null type before stripping the flag
        if (type.Value == SchemaValueType.Null)
            return "null";

        // Strip Null flag for nullable types (e.g., String | Null → String)
        var t = type.Value & ~SchemaValueType.Null;

        if (t == SchemaValueType.Array)
        {
            var itemsSchema = propSchema.GetItemsSchema();
            if (itemsSchema != null)
            {
                var itemType = SchemaToTypeString(itemsSchema, linkDefinitions);
                return $"{itemType}[]";
            }
            return "array";
        }

        switch (t)
        {
            case SchemaValueType.String: return "string";
            case SchemaValueType.Integer: return "integer";
            case SchemaValueType.Number: return "number";
            case SchemaValueType.Boolean: return "boolean";
            case SchemaValueType.Object: return "object";
            default: return "object";
        }
    }
}
