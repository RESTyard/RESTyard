using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RESTyard.Schema.Mermaid;
using RESTyard.Schema.Model;

namespace RESTyard.Schema.Markdown;

/// <summary>
/// Converts a <see cref="HypermediaApiSchema"/> to a Markdown API reference document.
/// </summary>
/// <remarks>
/// JSON Schema parsing limitations: Only the top-level <c>type</c> field of each property
/// in a JSON Schema <c>properties</c> object is read. Complex constructs such as
/// <c>$ref</c>, <c>allOf</c>/<c>anyOf</c>/<c>oneOf</c>, nested objects, and array
/// item types are not resolved — they display as <c>object</c>.
/// </remarks>
public static class MarkdownMapper
{
    /// <summary>
    /// Produces a Markdown API reference document from a hypermedia API schema.
    /// </summary>
    /// <param name="schema">The hypermedia API schema to document.</param>
    /// <param name="options">Optional settings to control what sections appear.
    /// When null, all sections are included.</param>
    /// <returns>A Markdown string containing the full API documentation.</returns>
    public static string ToDocumentation(this HypermediaApiSchema schema, MarkdownMapperOptions? options = null)
    {
        var opts = options ?? new MarkdownMapperOptions();
        var sb = new StringBuilder();

        AppendHeader(sb, schema);

        var orderedEntities = GetBfsOrderedEntities(schema);

        if (opts.IncludeTableOfContents && orderedEntities.Count > 0)
        {
            AppendTableOfContents(sb, orderedEntities);
        }

        if (opts.IncludeDiagram && schema.EntityTypes.Count > 0)
        {
            AppendDiagram(sb, schema);
        }

        foreach (var entity in orderedEntities)
        {
            AppendEntitySection(sb, entity);
        }

        return sb.ToString().TrimEnd();
    }

    private static void AppendHeader(StringBuilder sb, HypermediaApiSchema schema)
    {
        var title = schema.Title ?? "API Documentation";
        sb.AppendLine($"# {title}");

        if (schema.Description != null)
        {
            sb.AppendLine();
            sb.AppendLine(schema.Description);
        }

        if (schema.ApiVersion != null)
        {
            sb.AppendLine();
            sb.AppendLine($"**Version:** {schema.ApiVersion}");
        }

        if (schema.ExternalDocsUrl != null)
        {
            sb.AppendLine();
            sb.AppendLine($"**Documentation:** {schema.ExternalDocsUrl}");
        }
    }

    private static void AppendTableOfContents(StringBuilder sb, IReadOnlyList<EntityTypeSchema> entities)
    {
        sb.AppendLine();
        sb.AppendLine("## Table of Contents");
        sb.AppendLine();
        foreach (var entity in entities)
        {
            sb.AppendLine($"- [{entity.Name}](#{ToAnchor(entity.Name)})");
        }
    }

    private static void AppendDiagram(StringBuilder sb, HypermediaApiSchema schema)
    {
        sb.AppendLine();
        sb.AppendLine("## API Map");
        sb.AppendLine();
        sb.AppendLine("```mermaid");
        sb.AppendLine(schema.ToApiMap());
        sb.AppendLine("```");
    }

    private static void AppendEntitySection(StringBuilder sb, EntityTypeSchema entity)
    {
        sb.AppendLine();

        if (entity.IsDeprecated)
        {
            sb.AppendLine($"## **[Deprecated]** {entity.Name}");
            if (entity.DeprecationMessage != null)
            {
                sb.AppendLine();
                sb.AppendLine($"> {entity.DeprecationMessage}");
            }
        }
        else
        {
            sb.AppendLine($"## {entity.Name}");
        }

        if (entity.Title != null)
        {
            sb.AppendLine();
            sb.AppendLine(entity.Title);
        }

        if (entity.Description != null)
        {
            sb.AppendLine();
            sb.AppendLine(entity.Description);
        }

        if (entity.Classes.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine($"**Classes:** {string.Join(", ", entity.Classes.Select(c => $"`{c}`"))}");
        }

        AppendPropertiesTable(sb, entity);
        AppendLinksTable(sb, entity);
        AppendActionsTable(sb, entity);
        AppendEmbeddedEntitiesTable(sb, entity);
    }

    private static void AppendPropertiesTable(StringBuilder sb, EntityTypeSchema entity)
    {
        if (entity.PropertiesSchema is not { } propSchema
            || !propSchema.TryGetProperty("properties", out var props))
            return;

        var properties = props.EnumerateObject().ToList();
        if (properties.Count == 0)
            return;

        // Determine required fields
        var requiredFields = new HashSet<string>();
        if (propSchema.TryGetProperty("required", out var requiredArray))
        {
            foreach (var item in requiredArray.EnumerateArray())
            {
                var name = item.GetString();
                if (name != null)
                    requiredFields.Add(name);
            }
        }

        sb.AppendLine();
        sb.AppendLine("### Properties");
        sb.AppendLine();
        sb.AppendLine("| Property | Type | Required | Description |");
        sb.AppendLine("|---|---|---|---|");

        foreach (var prop in properties)
        {
            var typeName = prop.Value.TryGetProperty("type", out var t)
                ? t.GetString() ?? "object"
                : "object";
            var required = requiredFields.Contains(prop.Name) ? "yes" : "no";
            var description = prop.Value.TryGetProperty("description", out var d)
                ? d.GetString() ?? ""
                : "";
            sb.AppendLine($"| {prop.Name} | {typeName} | {required} | {description} |");
        }
    }

    private static void AppendLinksTable(StringBuilder sb, EntityTypeSchema entity)
    {
        if (entity.Links.Count == 0)
            return;

        sb.AppendLine();
        sb.AppendLine("### Links");
        sb.AppendLine();
        sb.AppendLine("| Relation | Target | Description |");
        sb.AppendLine("|---|---|---|");

        foreach (var link in entity.Links)
        {
            var rel = MermaidMapper.GetFirstRelation(link.Relations);
            var relDisplay = FormatRelation(rel, link.IsMandatory, link.IsDeprecated);
            var target = $"[{link.TargetName}](#{ToAnchor(link.TargetName)})";
            var description = link.Description ?? "";
            sb.AppendLine($"| {relDisplay} | {target} | {description} |");
        }
    }

    private static void AppendActionsTable(StringBuilder sb, EntityTypeSchema entity)
    {
        if (entity.Actions.Count == 0)
            return;

        sb.AppendLine();
        sb.AppendLine("### Actions");
        sb.AppendLine();
        sb.AppendLine("| Action | Description |");
        sb.AppendLine("|---|---|");

        foreach (var action in entity.Actions)
        {
            var nameDisplay = FormatActionName(action);
            var description = BuildActionDescription(action);
            sb.AppendLine($"| {nameDisplay} | {description} |");

            AppendActionParametersTable(sb, action);
        }
    }

    private static string FormatActionName(ActionDescription action)
    {
        var name = action.Name;
        if (action.IsDeprecated)
            name = $"**[Deprecated]** {name}";
        if (!action.IsMandatory)
            name = $"{name} *(optional)*";
        return name;
    }

    private static string BuildActionDescription(ActionDescription action)
    {
        var parts = new List<string>();
        if (action.Description != null)
            parts.Add(action.Description);
        if (action.ResultName != null)
            parts.Add($"Returns: [{action.ResultName}](#{ToAnchor(action.ResultName)})");
        return string.Join(" ", parts);
    }

    private static void AppendActionParametersTable(StringBuilder sb, ActionDescription action)
    {
        if (action.ParameterSchema is not { } paramSchema
            || !paramSchema.TryGetProperty("properties", out var props))
            return;

        var parameters = props.EnumerateObject().ToList();
        if (parameters.Count == 0)
            return;

        var requiredFields = new HashSet<string>();
        if (paramSchema.TryGetProperty("required", out var requiredArray))
        {
            foreach (var item in requiredArray.EnumerateArray())
            {
                var name = item.GetString();
                if (name != null)
                    requiredFields.Add(name);
            }
        }

        sb.AppendLine();
        sb.AppendLine("  | Parameter | Type | Required |");
        sb.AppendLine("  |---|---|---|");

        foreach (var prop in parameters)
        {
            var typeName = prop.Value.TryGetProperty("type", out var t)
                ? t.GetString() ?? "object"
                : "object";
            var required = requiredFields.Contains(prop.Name) ? "yes" : "no";
            sb.AppendLine($"  | {prop.Name} | {typeName} | {required} |");
        }
    }

    private static void AppendEmbeddedEntitiesTable(StringBuilder sb, EntityTypeSchema entity)
    {
        if (entity.EmbeddedEntities.Count == 0)
            return;

        sb.AppendLine();
        sb.AppendLine("### Embedded Entities");
        sb.AppendLine();
        sb.AppendLine("| Relation | Target | Collection | Description |");
        sb.AppendLine("|---|---|---|---|");

        foreach (var embedded in entity.EmbeddedEntities)
        {
            var rel = MermaidMapper.GetFirstRelation(embedded.Relations);
            var relDisplay = FormatRelation(rel, embedded.IsMandatory, embedded.IsDeprecated);
            var target = $"[{embedded.TargetName}](#{ToAnchor(embedded.TargetName)})";
            var collection = embedded.IsCollection ? "yes" : "no";
            var description = embedded.Description ?? "";
            sb.AppendLine($"| {relDisplay} | {target} | {collection} | {description} |");
        }
    }

    private static string FormatRelation(string rel, bool isMandatory, bool isDeprecated)
    {
        var display = rel;
        if (isDeprecated)
            display = $"**[Deprecated]** {display}";
        if (!isMandatory)
            display = $"{display} *(optional)*";
        return display;
    }

    private static IReadOnlyList<EntityTypeSchema> GetBfsOrderedEntities(HypermediaApiSchema schema)
    {
        if (schema.EntityTypes.Count == 0)
            return Array.Empty<EntityTypeSchema>();

        var nameToEntity = new Dictionary<string, EntityTypeSchema>();
        foreach (var entity in schema.EntityTypes)
        {
            nameToEntity[entity.Name] = entity;
        }

        var visited = new HashSet<string>();
        var result = new List<EntityTypeSchema>();
        var queue = new Queue<string>();

        if (nameToEntity.ContainsKey(schema.EntryPointName))
        {
            queue.Enqueue(schema.EntryPointName);
            visited.Add(schema.EntryPointName);
        }

        while (queue.Count > 0)
        {
            var name = queue.Dequeue();
            if (!nameToEntity.TryGetValue(name, out var entity))
                continue;

            result.Add(entity);

            foreach (var link in entity.Links)
            {
                if (!visited.Contains(link.TargetName))
                {
                    visited.Add(link.TargetName);
                    queue.Enqueue(link.TargetName);
                }
            }

            foreach (var embedded in entity.EmbeddedEntities)
            {
                if (!visited.Contains(embedded.TargetName))
                {
                    visited.Add(embedded.TargetName);
                    queue.Enqueue(embedded.TargetName);
                }
            }
        }

        // Append disconnected entities
        foreach (var entity in schema.EntityTypes)
        {
            if (!visited.Contains(entity.Name))
            {
                result.Add(entity);
            }
        }

        return result;
    }

    private static string ToAnchor(string name)
    {
        return name.ToLowerInvariant();
    }
}
