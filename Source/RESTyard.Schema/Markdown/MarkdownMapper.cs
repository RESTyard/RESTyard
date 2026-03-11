using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Json.Schema;
using RESTyard.Schema.Mermaid;
using RESTyard.Schema.Model;

namespace RESTyard.Schema.Markdown;

/// <summary>
/// Converts a <see cref="HypermediaApiSchema"/> to a Markdown API reference document.
/// </summary>
/// <remarks>
/// Property types are extracted from <c>JsonSchema.Net</c> keywords. <c>$ref</c> references
/// resolve to definition names, array <c>items</c> are shown as <c>T[]</c>, and nullable types
/// strip the <c>Null</c> flag. <c>allOf</c>/<c>anyOf</c>/<c>oneOf</c> compositions are not yet
/// resolved and display as <c>object</c>.
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

        if (opts.IncludeTableOfContents && (orderedEntities.Count > 0 || schema.Definitions.Count > 0))
        {
            AppendTableOfContents(sb, orderedEntities, schema.Definitions);
        }

        if (opts.IncludeDiagram && schema.EntityTypes.Count > 0)
        {
            AppendDiagram(sb, schema);
        }

        var incomingLinks = CollectIncomingLinks(schema);

        foreach (var entity in orderedEntities)
        {
            AppendEntitySection(sb, entity, incomingLinks);
        }

        if (schema.Definitions.Count > 0)
        {
            var usages = CollectDefinitionUsages(schema);
            AppendDefinitionsSection(sb, schema.Definitions, usages);
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

        if (!string.IsNullOrEmpty(schema.EntryPointName))
        {
            sb.AppendLine();
            sb.AppendLine($"**Entry Point:** [{schema.EntryPointName}](#{ToAnchor(schema.EntryPointName)})");
        }
    }

    private static void AppendTableOfContents(
        StringBuilder sb,
        IReadOnlyList<EntityTypeSchema> entities,
        IDictionary<string, JsonSchema> definitions)
    {
        sb.AppendLine();
        sb.AppendLine("## Table of Contents");
        sb.AppendLine();

        foreach (var entity in entities)
        {
            sb.AppendLine($"- [{entity.Name}](#{ToAnchor(entity.Name)})");
        }

        if (definitions.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("**Definitions**");
            sb.AppendLine();
            foreach (var def in definitions)
            {
                sb.AppendLine($"- [{def.Key}](#definition-{def.Key.ToLowerInvariant()})");
            }
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

    private static void AppendEntitySection(
        StringBuilder sb,
        EntityTypeSchema entity,
        Dictionary<string, List<string>> incomingLinks)
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
            sb.AppendLine("#### Title");
            sb.AppendLine();
            sb.AppendLine(entity.Title);
        }

        if (entity.Description != null)
        {
            sb.AppendLine();
            sb.AppendLine("#### Description");
            sb.AppendLine();
            sb.AppendLine(entity.Description);
        }

        if (entity.Classes.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("#### Classes");
            sb.AppendLine();
            foreach (var c in entity.Classes)
            {
                sb.AppendLine($"- `{c}`");
            }
        }

        AppendPropertiesTable(sb, entity);
        AppendLinksTable(sb, entity);
        AppendActionsTable(sb, entity);
        AppendEmbeddedEntitiesTable(sb, entity);
        AppendReferencedBy(sb, entity, incomingLinks);
    }

    private static void AppendPropertiesTable(StringBuilder sb, EntityTypeSchema entity)
    {
        if (entity.PropertiesSchema is not { } propSchema
            || propSchema.GetProperties() is not { } props
            || props.Count == 0)
            return;

        var requiredFields = new HashSet<string>(propSchema.GetRequired() ?? Array.Empty<string>());

        sb.AppendLine();
        sb.AppendLine($"<a id=\"{ToAnchor(entity.Name)}-properties\"></a>");
        sb.AppendLine();
        sb.AppendLine("### Properties");
        sb.AppendLine();
        sb.AppendLine("| Property | Type | Required | Description |");
        sb.AppendLine("|---|---|---|---|");

        foreach (var prop in props)
        {
            var typeName = JsonSchemaExtensions.SchemaToLinkedTypeString(prop.Value);
            var required = requiredFields.Contains(prop.Key) ? "yes" : "no";
            var description = BuildPropertyDescription(prop.Value);
            sb.AppendLine($"| {prop.Key} | {typeName} | {required} | {description} |");
        }
    }

    private static void AppendLinksTable(StringBuilder sb, EntityTypeSchema entity)
    {
        if (entity.Links.Count == 0)
            return;

        sb.AppendLine();
        sb.AppendLine($"<a id=\"{ToAnchor(entity.Name)}-links\"></a>");
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
        sb.AppendLine($"<a id=\"{ToAnchor(entity.Name)}-actions\"></a>");
        sb.AppendLine();
        sb.AppendLine("### Actions");
        sb.AppendLine();
        sb.AppendLine("| Action | Description | Links to |");
        sb.AppendLine("|---|---|---|");

        foreach (var action in entity.Actions)
        {
            var nameDisplay = FormatActionName(action);
            var description = action.Description ?? "";
            var returns = action.ResultName != null
                ? $"[{action.ResultName}](#{ToAnchor(action.ResultName)})"
                : "";
            sb.AppendLine($"| {nameDisplay} | {description} | {returns} |");

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

    private static void AppendActionParametersTable(StringBuilder sb, ActionDescription action)
    {
        if (action.ParameterSchema is not { } paramSchema
            || paramSchema.GetProperties() is not { } props
            || props.Count == 0)
            return;

        var requiredFields = new HashSet<string>(paramSchema.GetRequired() ?? Array.Empty<string>());

        sb.AppendLine();
        sb.AppendLine("  | Parameter | Type | Required | Description |");
        sb.AppendLine("  |---|---|---|---|");

        foreach (var prop in props)
        {
            var typeName = JsonSchemaExtensions.SchemaToLinkedTypeString(prop.Value);
            var required = requiredFields.Contains(prop.Key) ? "yes" : "no";
            var description = BuildPropertyDescription(prop.Value);
            sb.AppendLine($"  | {prop.Key} | {typeName} | {required} | {description} |");
        }
    }

    private static void AppendEmbeddedEntitiesTable(StringBuilder sb, EntityTypeSchema entity)
    {
        if (entity.EmbeddedEntities.Count == 0)
            return;

        sb.AppendLine();
        sb.AppendLine($"<a id=\"{ToAnchor(entity.Name)}-embedded\"></a>");
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

    private static string BuildPropertyDescription(JsonSchema propSchema)
    {
        var parts = new List<string>();

        var desc = propSchema.GetDescription();
        if (desc != null)
            parts.Add(desc);

        var format = propSchema.GetFormat();
        if (format != null)
            parts.Add($"Format: `{format}`");

        var enumValues = propSchema.GetEnum();
        if (enumValues != null && enumValues.Count > 0)
        {
            var values = string.Join(", ", enumValues.Select(v => $"`{v?.ToString() ?? "null"}`"));
            parts.Add($"Values: {values}");
        }

        var defaultValue = propSchema.GetDefault();
        if (defaultValue != null)
            parts.Add($"Default: `{defaultValue}`");

        return string.Join(". ", parts);
    }

    private static Dictionary<string, List<string>> CollectIncomingLinks(HypermediaApiSchema schema)
    {
        var incoming = new Dictionary<string, List<string>>();

        foreach (var entity in schema.EntityTypes)
        {
            foreach (var link in entity.Links)
            {
                var rel = MermaidMapper.GetFirstRelation(link.Relations);
                if (string.Equals(rel, "self", StringComparison.OrdinalIgnoreCase))
                    continue;

                AddUsage(incoming, link.TargetName,
                    $"[{entity.Name}](#{ToAnchor(entity.Name)}-links) (link: {rel})");
            }

            foreach (var embedded in entity.EmbeddedEntities)
            {
                var rel = MermaidMapper.GetFirstRelation(embedded.Relations);
                AddUsage(incoming, embedded.TargetName,
                    $"[{entity.Name}](#{ToAnchor(entity.Name)}-embedded) (embedded: {rel})");
            }

            foreach (var action in entity.Actions)
            {
                if (action.ResultName != null)
                {
                    AddUsage(incoming, action.ResultName,
                        $"[{entity.Name}](#{ToAnchor(entity.Name)}-actions) (action: {action.Name})");
                }
            }
        }

        return incoming;
    }

    private static void AppendReferencedBy(
        StringBuilder sb,
        EntityTypeSchema entity,
        Dictionary<string, List<string>> incomingLinks)
    {
        if (!incomingLinks.TryGetValue(entity.Name, out var refs) || refs.Count == 0)
            return;

        sb.AppendLine();
        sb.AppendLine("**Referenced by:**");
        sb.AppendLine();
        foreach (var r in refs)
        {
            sb.AppendLine($"- {r}");
        }
    }

    /// <summary>
    /// Scans all entity properties and action parameters for <c>$ref</c> references,
    /// returning a map from definition name to the list of places that reference it.
    /// </summary>
    private static Dictionary<string, List<string>> CollectDefinitionUsages(HypermediaApiSchema schema)
    {
        var usages = new Dictionary<string, List<string>>();

        foreach (var entity in schema.EntityTypes)
        {
            // Scan entity properties
            if (entity.PropertiesSchema is { } propSchema
                && propSchema.GetProperties() is { } props)
            {
                foreach (var prop in props)
                {
                    var defName = ExtractRefDefinitionName(prop.Value);
                    if (defName != null)
                        AddUsage(usages, defName, $"[{entity.Name}](#{ToAnchor(entity.Name)})");
                }
            }

            // Scan action parameters
            foreach (var action in entity.Actions)
            {
                if (action.ParameterSchema is { } paramSchema
                    && paramSchema.GetProperties() is { } paramProps)
                {
                    foreach (var prop in paramProps)
                    {
                        var defName = ExtractRefDefinitionName(prop.Value);
                        if (defName != null)
                            AddUsage(usages, defName, $"[{entity.Name} → {action.Name}](#{ToAnchor(entity.Name)})");
                    }
                }
            }
        }

        return usages;
    }

    private static string? ExtractRefDefinitionName(JsonSchema propSchema)
    {
        var refUri = propSchema.GetRef();
        if (refUri != null)
        {
            var refString = refUri.OriginalString;
            var lastSlash = refString.LastIndexOf('/');
            if (lastSlash >= 0 && lastSlash < refString.Length - 1)
                return refString.Substring(lastSlash + 1);
        }

        // Also check array items for $ref
        if (propSchema.GetItemsSchema() is { } itemsSchema)
            return ExtractRefDefinitionName(itemsSchema);

        return null;
    }

    private static void AddUsage(Dictionary<string, List<string>> usages, string defName, string usage)
    {
        if (!usages.TryGetValue(defName, out var list))
        {
            list = new List<string>();
            usages[defName] = list;
        }
        if (!list.Contains(usage))
            list.Add(usage);
    }

    private static void AppendDefinitionsSection(
        StringBuilder sb,
        IDictionary<string, JsonSchema> definitions,
        Dictionary<string, List<string>> usages)
    {
        sb.AppendLine();
        sb.AppendLine("## Definitions");

        foreach (var def in definitions)
        {
            sb.AppendLine();
            sb.AppendLine($"### Definition: {def.Key}");

            var description = def.Value.GetDescription();
            if (description != null)
            {
                sb.AppendLine();
                sb.AppendLine(description);
            }

            if (usages.TryGetValue(def.Key, out var refs) && refs.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine($"**Used by:** {string.Join(", ", refs)}");
            }

            var props = def.Value.GetProperties();
            if (props != null && props.Count > 0)
            {
                var requiredFields = new HashSet<string>(def.Value.GetRequired() ?? Array.Empty<string>());

                sb.AppendLine();
                sb.AppendLine("| Property | Type | Required | Description |");
                sb.AppendLine("|---|---|---|---|");

                foreach (var prop in props)
                {
                    var typeName = JsonSchemaExtensions.SchemaToLinkedTypeString(prop.Value);
                    var required = requiredFields.Contains(prop.Key) ? "yes" : "no";
                    var propDescription = BuildPropertyDescription(prop.Value);
                    sb.AppendLine($"| {prop.Key} | {typeName} | {required} | {propDescription} |");
                }
            }
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
