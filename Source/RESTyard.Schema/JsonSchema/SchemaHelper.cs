using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace RESTyard.Schema;

/// <summary>
/// Helper used by source-generated schema code to compose individual property
/// schemas (from <see cref="IJsonSchemaFactory"/>) into a single
/// <c>{"type":"object","properties":{...}}</c> JSON Schema.
/// </summary>
public static class SchemaHelper
{
    /// <summary>
    /// Builds a JSON Schema of type "object" whose "properties" map contains
    /// the provided property name → schema pairs.
    /// </summary>
    public static Json.Schema.JsonSchema BuildPropertiesSchema(Dictionary<string, JsonDocument> propertySchemas)
    {
        var sb = new StringBuilder();
        sb.Append("{\"type\":\"object\",\"properties\":{");

        var first = true;
        foreach (var kvp in propertySchemas)
        {
            if (!first)
            {
                sb.Append(',');
            }

            first = false;
            sb.Append('"').Append(EscapeJsonString(kvp.Key)).Append("\":");
            sb.Append(kvp.Value.RootElement.GetRawText());
        }

        sb.Append("}}");

        return Json.Schema.JsonSchema.FromText(sb.ToString());
    }

    private static string EscapeJsonString(string value)
        => value.Replace("\\", "\\\\").Replace("\"", "\\\"");
}
