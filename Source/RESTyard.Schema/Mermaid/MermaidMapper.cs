using System;
using System.Text;
using RESTyard.Schema.Model;

namespace RESTyard.Schema.Mermaid;

/// <summary>
/// Converts a <see cref="HypermediaApiSchema"/> to Mermaid diagram strings.
/// </summary>
/// <remarks>
/// JSON Schema parsing limitations: Only the top-level <c>type</c> field of each property
/// in a JSON Schema <c>properties</c> object is read. Complex constructs such as
/// <c>$ref</c>, <c>allOf</c>/<c>anyOf</c>/<c>oneOf</c>, nested objects, and array
/// item types are not resolved — they display as <c>object</c>.
/// </remarks>
public static class MermaidMapper
{
    /// <summary>
    /// Produces a Mermaid entity relationship graph (<c>graph LR</c>) showing
    /// entity types as nodes and links/embedded entities as labeled edges.
    /// </summary>
    /// <param name="schema">The hypermedia API schema to visualize.</param>
    /// <returns>A Mermaid diagram string in <c>graph LR</c> format.</returns>
    public static string ToEntityGraph(HypermediaApiSchema schema)
    {
        var sb = new StringBuilder();
        sb.AppendLine("graph LR");

        if (schema.EntityTypes.Count == 0)
        {
            sb.AppendLine("    NoEntities[\"No entities defined\"]");
            return sb.ToString().TrimEnd();
        }

        foreach (var entity in schema.EntityTypes)
        {
            sb.AppendLine($"    {entity.Name}[\"{entity.Name}\"]");
        }

        var hasEdges = false;
        foreach (var entity in schema.EntityTypes)
        {
            foreach (var link in entity.Links)
            {
                var rel = GetFirstRelation(link.Relations);
                if (string.Equals(rel, "self", StringComparison.OrdinalIgnoreCase))
                    continue;

                AppendEdgeSeparatorOnce(sb, ref hasEdges);
                sb.AppendLine($"    {entity.Name} -- \"{rel}\" --> {link.TargetName}");
            }

            foreach (var embedded in entity.EmbeddedEntities)
            {
                var rel = GetFirstRelation(embedded.Relations);

                AppendEdgeSeparatorOnce(sb, ref hasEdges);
                sb.AppendLine($"    {entity.Name} -- \"{rel}\" --> {embedded.TargetName}");
            }
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Produces a Mermaid class diagram (<c>classDiagram</c>) showing entity types
    /// with their properties (from <c>PropertiesSchema</c>) and actions.
    /// </summary>
    /// <remarks>
    /// Property types are extracted from the JSON Schema <c>type</c> field only.
    /// Complex types (arrays, nested objects, <c>$ref</c>) are shown as <c>object</c>.
    /// </remarks>
    /// <param name="schema">The hypermedia API schema to visualize.</param>
    /// <returns>A Mermaid diagram string in <c>classDiagram</c> format.</returns>
    public static string ToClassDiagram(HypermediaApiSchema schema)
    {
        var sb = new StringBuilder();
        sb.AppendLine("classDiagram");

        if (schema.EntityTypes.Count == 0)
        {
            sb.AppendLine("    class NoEntities {");
            sb.AppendLine("        <<No entities defined>>");
            sb.AppendLine("    }");
            return sb.ToString().TrimEnd();
        }

        foreach (var entity in schema.EntityTypes)
        {
            sb.AppendLine($"    class {entity.Name} {{");

            if (entity.PropertiesSchema is { } propSchema
                && propSchema.TryGetProperty("properties", out var props))
            {
                foreach (var prop in props.EnumerateObject())
                {
                    var typeName = prop.Value.TryGetProperty("type", out var t)
                        ? t.GetString() ?? "object"
                        : "object";
                    sb.AppendLine($"        +{typeName} {prop.Name}");
                }
            }

            foreach (var action in entity.Actions)
            {
                var paramIndicator = action.ParameterSchema != null ? "params" : "";
                sb.AppendLine($"        +{action.Name}({paramIndicator})");
            }

            sb.AppendLine("    }");
        }

        foreach (var entity in schema.EntityTypes)
        {
            foreach (var link in entity.Links)
            {
                var rel = GetFirstRelation(link.Relations);
                if (string.Equals(rel, "self", StringComparison.OrdinalIgnoreCase))
                    continue;

                sb.AppendLine($"    {entity.Name} --> {link.TargetName} : {rel}");
            }

            foreach (var embedded in entity.EmbeddedEntities)
            {
                var rel = GetFirstRelation(embedded.Relations);
                sb.AppendLine($"    {entity.Name} --> {embedded.TargetName} : {rel}");
            }
        }

        return sb.ToString().TrimEnd();
    }

    private static string GetFirstRelation(System.Collections.Generic.IReadOnlyList<string> relations)
    {
        return relations.Count > 0 ? relations[0] : "related";
    }

    private static void AppendEdgeSeparatorOnce(StringBuilder sb, ref bool hasEdges)
    {
        if (!hasEdges)
        {
            sb.AppendLine();
            hasEdges = true;
        }
    }
}
