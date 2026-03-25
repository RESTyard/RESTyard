using System;
using System.Text;
using RESTyard.Schema.Model;

namespace RESTyard.Schema.Mermaid;

/// <summary>
/// Converts a <see cref="HypermediaApiSchema"/> to Mermaid diagram strings.
/// </summary>
/// <remarks>
/// Property types are extracted from <c>JsonSchema.Net</c> keywords. <c>$ref</c> references
/// resolve to definition names, array <c>items</c> are shown as <c>T[]</c>, and nullable types
/// strip the <c>Null</c> flag. <c>allOf</c>/<c>anyOf</c>/<c>oneOf</c> compositions are not yet
/// resolved and display as <c>object</c>.
/// </remarks>
public static class MermaidMapper
{
    /// <summary>
    /// Produces a Mermaid API Map (<c>graph LR</c>) showing
    /// entity types as nodes and links/embedded entities as labeled edges.
    /// </summary>
    /// <param name="schema">The hypermedia API schema to visualize.</param>
    /// <returns>A Mermaid diagram string in <c>graph LR</c> format.</returns>
    public static string ToApiMap(this HypermediaApiSchema schema)
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
            var label = entity.AccessGroups is { Count: > 0 } groups
                ? $"{entity.Name} [{string.Join(", ", groups)}]"
                : entity.Name;
            sb.AppendLine($"    {entity.Name}[\"{label}\"]");
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

            foreach (var action in entity.Actions)
            {
                if (action.ResultName != null)
                {
                    AppendEdgeSeparatorOnce(sb, ref hasEdges);
                    sb.AppendLine($"    {entity.Name} -. \"action: {action.Name}\" .-> {action.ResultName}");
                }
            }
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Produces a Mermaid class diagram (<c>classDiagram</c>) showing entity types
    /// with their properties (from <c>PropertiesSchema</c>) and actions.
    /// </summary>
    /// <remarks>
    /// Property types are extracted from the JSON Schema <c>type</c> keyword.
    /// Complex types (arrays, nested objects, <c>$ref</c>) are shown as <c>object</c>.
    /// </remarks>
    /// <param name="schema">The hypermedia API schema to visualize.</param>
    /// <param name="options">Optional settings to control what details appear in class boxes.
    /// When null, all properties and actions are included.</param>
    /// <returns>A Mermaid diagram string in <c>classDiagram</c> format.</returns>
    public static string ToClassDiagram(this HypermediaApiSchema schema, MermaidMapperOptions? options = null)
    {
        var opts = options ?? new MermaidMapperOptions();
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

            if (entity.AccessGroups is { Count: > 0 } groups)
            {
                sb.AppendLine($"        access: {string.Join(", ", groups)}");
            }

            if (opts.IncludeProperties
                && entity.PropertiesSchema is { } propDoc
                && propDoc.ToJsonSchema().GetProperties() is { } props)
            {
                foreach (var prop in props)
                {
                    var typeName = JsonSchemaExtensions.SchemaToTypeString(prop.Value);
                    sb.AppendLine($"        +{typeName} {prop.Key}");
                }
            }

            if (opts.IncludeActions)
            {
                foreach (var action in entity.Actions)
                {
                    var paramIndicator = action.ParameterSchema is not null ? "params" : "";
                    var returnType = action.ResultName != null ? $" {action.ResultName}" : "";
                    sb.AppendLine($"        +{action.Name}({paramIndicator}){returnType}");
                }
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

    internal static string GetFirstRelation(System.Collections.Generic.IReadOnlyList<string> relations)
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
