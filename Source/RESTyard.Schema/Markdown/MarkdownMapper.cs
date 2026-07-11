using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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
        var accessGroupUsages = CollectAccessGroupUsages(schema);

        if (opts.IncludeTableOfContents && (orderedEntities.Count > 0 || schema.Definitions.Count > 0))
        {
            AppendTableOfContents(sb, orderedEntities, schema.Definitions, accessGroupUsages);
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

        if (accessGroupUsages.Count > 0)
        {
            AppendAccessGroupsSection(sb, accessGroupUsages);
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
        IDictionary<string, JsonDocument> definitions,
        SortedDictionary<string, List<string>> accessGroupUsages)
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
                var defSchema = def.Value.ToJsonSchema();
                var displayName = GetDisplayNameFromSchema(defSchema) ?? CleanTypeName(def.Key);
                var anchor = ToAnchor(def.Key);
                sb.AppendLine($"- [{displayName}](#definition-{anchor})");
            }
        }

        if (accessGroupUsages.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("**Access Groups**");
            sb.AppendLine();
            foreach (var group in accessGroupUsages.Keys)
            {
                sb.AppendLine($"- [{group}](#access-group-{ToAnchor(group)})");
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

        AppendAccessGroups(sb, entity.AccessGroups);

        AppendPropertiesTable(sb, entity);
        AppendLinksTable(sb, entity);
        AppendActions(sb, entity);
        AppendEmbeddedEntitiesTable(sb, entity);
        AppendReferencedBy(sb, entity, incomingLinks);
    }

    private static void AppendPropertiesTable(StringBuilder sb, EntityTypeSchema entity)
    {
        if (entity.PropertiesSchema is not { } propDoc)
            return;

        var propSchema = propDoc.ToJsonSchema();
        if (propSchema.GetProperties() is not { } props || props.Count == 0)
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
            var typeName = JsonSchemaExtensions.SchemaToLinkedTypeString(prop.Value, propSchema);
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
        sb.AppendLine("| Relation | Target | Access Groups | Description |");
        sb.AppendLine("|---|---|---|---|");

        foreach (var link in entity.Links)
        {
            var rel = MermaidMapper.GetFirstRelation(link.Relations);
            var relDisplay = FormatRelation(rel, link.IsMandatory, link.IsDeprecated);
            var target = link.TargetName != null
                ? $"[{link.TargetName}](#{ToAnchor(link.TargetName)})"
                : link.MediaTypes.Count > 0
                    ? $"*external* (`{string.Join("`, `", link.MediaTypes)}`)"
                    : "*external*";
            var accessGroups = FormatAccessGroupsInline(link.AccessGroups);
            var description = link.Description ?? "";
            sb.AppendLine($"| {relDisplay} | {target} | {accessGroups} | {description} |");
        }
    }

    private static void AppendActions(StringBuilder sb, EntityTypeSchema entity)
    {
        if (entity.Actions.Count == 0)
            return;

        sb.AppendLine();
        sb.AppendLine("### Actions");

        foreach (var action in entity.Actions)
        {
            var actionAnchor = $"{ToAnchor(entity.Name)}-{ToAnchor(action.Name)}";
            sb.AppendLine();
            sb.AppendLine($"<a id=\"{actionAnchor}\"></a>");
            sb.AppendLine();

            var nameDisplay = FormatActionName(action);
            sb.AppendLine($"#### {nameDisplay}");

            if (action.Title != null)
            {
                sb.AppendLine();
                sb.AppendLine(action.Title);
            }

            if (action.Description != null)
            {
                sb.AppendLine();
                sb.AppendLine(action.Description);
            }

            if (action.ResultName != null)
            {
                sb.AppendLine();
                sb.AppendLine($"**Returns:** [{action.ResultName}](#{ToAnchor(action.ResultName)})");
            }

            AppendAccessGroups(sb, action.AccessGroups);

            if (action.IsFileUpload)
            {
                sb.AppendLine();
                sb.AppendLine("**File upload** (`multipart/form-data`)");
            }

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
        if (action.ParameterSchema is not { } paramDoc)
            return;

        var paramSchema = paramDoc.ToJsonSchema();
        if (paramSchema.GetProperties() is not { } props || props.Count == 0)
            return;

        var requiredFields = new HashSet<string>(paramSchema.GetRequired() ?? Array.Empty<string>());

        sb.AppendLine();
        sb.AppendLine("**Parameters:**");
        sb.AppendLine();
        sb.AppendLine("| Parameter | Type | Required | Description |");
        sb.AppendLine("|---|---|---|---|");

        foreach (var prop in props)
        {
            var typeName = JsonSchemaExtensions.SchemaToLinkedTypeString(prop.Value, paramSchema);
            var required = requiredFields.Contains(prop.Key) ? "yes" : "no";
            var description = BuildPropertyDescription(prop.Value);
            sb.AppendLine($"| {prop.Key} | {typeName} | {required} | {description} |");
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
        sb.AppendLine("| Relation | Target | Collection | Access Groups | Description |");
        sb.AppendLine("|---|---|---|---|---|");

        foreach (var embedded in entity.EmbeddedEntities)
        {
            var rel = MermaidMapper.GetFirstRelation(embedded.Relations);
            var relDisplay = FormatRelation(rel, embedded.IsMandatory, embedded.IsDeprecated);
            var target = $"[{embedded.TargetName}](#{ToAnchor(embedded.TargetName)})";
            var collection = embedded.IsCollection ? "yes" : "no";
            var accessGroups = FormatAccessGroupsInline(embedded.AccessGroups);
            var description = embedded.Description ?? "";
            sb.AppendLine($"| {relDisplay} | {target} | {collection} | {accessGroups} | {description} |");
        }
    }

    private static string BuildPropertyDescription(Json.Schema.JsonSchema propSchema)
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

                if (link.TargetName == null)
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
                        $"[{entity.Name} → {action.Name}](#{ToAnchor(entity.Name)}-{ToAnchor(action.Name)}) (action result)");
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
            if (entity.PropertiesSchema is { } propDoc
                && propDoc.ToJsonSchema().GetProperties() is { } props)
            {
                foreach (var prop in props)
                {
                    var defName = ExtractRefDefinitionName(prop.Value);
                    if (defName != null)
                        AddUsage(usages, defName.ToLowerInvariant(), $"[{entity.Name}](#{ToAnchor(entity.Name)})");
                }
            }

            // Scan action parameters
            foreach (var action in entity.Actions)
            {
                if (action.ParameterSchema is { } paramDoc
                    && paramDoc.ToJsonSchema().GetProperties() is { } paramProps)
                {
                    foreach (var prop in paramProps)
                    {
                        var defName = ExtractRefDefinitionName(prop.Value);
                        if (defName != null)
                            AddUsage(usages, defName.ToLowerInvariant(), $"[{entity.Name} → {action.Name}](#{ToAnchor(entity.Name)}-{ToAnchor(action.Name)})");
                    }
                }
            }
        }

        return usages;
    }

    private static string? ExtractRefDefinitionName(Json.Schema.JsonSchema propSchema)
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

    private static void AddUsage(IDictionary<string, List<string>> usages, string defName, string usage)
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
        IDictionary<string, JsonDocument> definitions,
        Dictionary<string, List<string>> usages)
    {
        sb.AppendLine();
        sb.AppendLine("## Definitions");

        foreach (var def in definitions)
        {
            var rawKey = def.Key; // $defs key (e.g., "addressTo", "country")
            var defSchema = def.Value.ToJsonSchema();
            var displayName = GetDisplayNameFromSchema(defSchema) ?? CleanTypeName(rawKey);
            var anchor = ToAnchor(rawKey);

            sb.AppendLine();
            sb.AppendLine($"<a id=\"definition-{anchor}\"></a>");
            sb.AppendLine();
            sb.AppendLine($"### Definition: {displayName}");

            var description = defSchema.GetDescription();
            if (description != null)
            {
                sb.AppendLine();
                sb.AppendLine(description);
            }

            // Usages are keyed by the lowercase $defs key
            if (usages.TryGetValue(anchor, out var refs) && refs.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("**Referenced by:**");
                sb.AppendLine();
                foreach (var r in refs)
                {
                    sb.AppendLine($"- {r}");
                }
            }

            var props = defSchema.GetProperties();
            if (props != null && props.Count > 0)
            {
                var requiredFields = new HashSet<string>(defSchema.GetRequired() ?? Array.Empty<string>());

                sb.AppendLine();
                sb.AppendLine("| Property | Type | Required | Description |");
                sb.AppendLine("|---|---|---|---|");

                foreach (var prop in props)
                {
                    var typeName = JsonSchemaExtensions.SchemaToLinkedTypeString(prop.Value, defSchema);
                    var required = requiredFields.Contains(prop.Key) ? "yes" : "no";
                    var propDescription = BuildPropertyDescription(prop.Value);
                    sb.AppendLine($"| {prop.Key} | {typeName} | {required} | {propDescription} |");
                }
            }
        }
    }

    private static void AppendAccessGroups(StringBuilder sb, IReadOnlyList<string>? accessGroups)
    {
        if (accessGroups == null || accessGroups.Count == 0)
            return;

        var links = string.Join(", ", accessGroups.Select(g => $"[{g}](#access-group-{ToAnchor(g)})"));
        sb.AppendLine();
        sb.AppendLine($"**Access Groups:** {links}");
    }

    /// <summary>
    /// Collects all access group usages across entities, actions, links, and embedded entities.
    /// Returns a sorted dictionary from group name to list of Markdown links pointing to the referencing elements.
    /// </summary>
    private static SortedDictionary<string, List<string>> CollectAccessGroupUsages(HypermediaApiSchema schema)
    {
        var usages = new SortedDictionary<string, List<string>>(StringComparer.Ordinal);

        foreach (var entity in schema.EntityTypes)
        {
            if (entity.AccessGroups is { Count: > 0 } entityGroups)
            {
                foreach (var group in entityGroups)
                {
                    AddUsage(usages, group,
                        $"[{entity.Name}](#{ToAnchor(entity.Name)}) (entity)");
                }
            }

            foreach (var action in entity.Actions)
            {
                if (action.AccessGroups is { Count: > 0 } actionGroups)
                {
                    var actionAnchor = $"{ToAnchor(entity.Name)}-{ToAnchor(action.Name)}";
                    foreach (var group in actionGroups)
                    {
                        AddUsage(usages, group,
                            $"[{entity.Name} → {action.Name}](#{actionAnchor}) (action)");
                    }
                }
            }

            foreach (var link in entity.Links)
            {
                if (link.AccessGroups is { Count: > 0 } linkGroups)
                {
                    var rel = MermaidMapper.GetFirstRelation(link.Relations);
                    foreach (var group in linkGroups)
                    {
                        AddUsage(usages, group,
                            $"[{entity.Name}](#{ToAnchor(entity.Name)}-links) (link: {rel})");
                    }
                }
            }

            foreach (var embedded in entity.EmbeddedEntities)
            {
                if (embedded.AccessGroups is { Count: > 0 } embeddedGroups)
                {
                    var rel = MermaidMapper.GetFirstRelation(embedded.Relations);
                    foreach (var group in embeddedGroups)
                    {
                        AddUsage(usages, group,
                            $"[{entity.Name}](#{ToAnchor(entity.Name)}-embedded) (embedded: {rel})");
                    }
                }
            }
        }

        return usages;
    }

    private static void AppendAccessGroupsSection(
        StringBuilder sb,
        SortedDictionary<string, List<string>> accessGroupUsages)
    {
        sb.AppendLine();
        sb.AppendLine("## Access Groups");

        foreach (var kvp in accessGroupUsages)
        {
            var group = kvp.Key;
            var refs = kvp.Value;

            sb.AppendLine();
            sb.AppendLine($"<a id=\"access-group-{ToAnchor(group)}\"></a>");
            sb.AppendLine();
            sb.AppendLine($"### {group}");
            sb.AppendLine();

            foreach (var r in refs)
            {
                sb.AppendLine($"- {r}");
            }
        }
    }

    private static string FormatAccessGroupsInline(IReadOnlyList<string>? accessGroups)
    {
        if (accessGroups == null || accessGroups.Count == 0)
            return "";

        return string.Join(", ", accessGroups.Select(g => $"[{g}](#access-group-{ToAnchor(g)})"));
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
                if (link.TargetName != null && !visited.Contains(link.TargetName))
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

    /// <summary>
    /// Extracts a clean display name from a definition schema's $id URI (e.g., "type:MyApp.Address" → "Address").
    /// Returns null if no $id is present.
    /// </summary>
    private static string? GetDisplayNameFromSchema(Json.Schema.JsonSchema schema)
    {
        var id = schema.Keywords?.OfType<Json.Schema.IdKeyword>().FirstOrDefault()?.Id;
        if (id == null) return null;

        var idString = id.OriginalString;
        const string prefix = "type:";
        var fullName = idString.StartsWith(prefix) ? idString.Substring(prefix.Length) : idString;
        return CleanTypeName(fullName);
    }

    private static string ToAnchor(string name)
    {
        // Remove characters invalid in HTML anchor IDs
        return name.ToLowerInvariant()
            .Replace("<", "")
            .Replace(">", "")
            .Replace(",", "")
            .Replace(" ", "-");
    }

    /// <summary>
    /// Cleans a CLR type name for display. Strips namespace, simplifies generic types
    /// (e.g., "SortParameter`1[[...]]" → "SortParameter&lt;CustomerSortProperties&gt;").
    /// </summary>
    internal static string CleanTypeName(string fullTypeName)
    {
        // Strip assembly-qualified generic args: `1[[Namespace.Type, Assembly, ...]] → <Type>
        var name = fullTypeName;

        // Handle generic types: SortParameter`1[[CarShack.Hypermedia.CustomerSortProperties, CarShack, ...]]
        var backtickIndex = name.IndexOf('`');
        if (backtickIndex >= 0)
        {
            var baseName = name.Substring(0, backtickIndex);
            // Extract type args from [[...]]
            var argsStart = name.IndexOf("[[", backtickIndex, StringComparison.Ordinal);
            if (argsStart >= 0)
            {
                var argNames = new List<string>();
                var remaining = name.Substring(argsStart);
                // Split on ],[  to get individual type args
                var args = remaining.Split(new[] { "],[" }, StringSplitOptions.None);
                foreach (var arg in args)
                {
                    // Clean up brackets and extract just the type name
                    var cleaned = arg.Trim('[', ']');
                    // Take only the type name (before first comma = assembly separator)
                    var commaIdx = cleaned.IndexOf(',');
                    var typePart = commaIdx >= 0 ? cleaned.Substring(0, commaIdx) : cleaned;
                    // Take just the simple name (after last dot)
                    var lastDot = typePart.LastIndexOf('.');
                    var simpleName = lastDot >= 0 ? typePart.Substring(lastDot + 1) : typePart;
                    argNames.Add(simpleName);
                }
                // Take simple name of base type too
                var baseLastDot = baseName.LastIndexOf('.');
                var simpleBase = baseLastDot >= 0 ? baseName.Substring(baseLastDot + 1) : baseName;
                return $"{simpleBase}<{string.Join(", ", argNames)}>";
            }
        }

        // Non-generic: just take the simple name after last dot
        var dotIndex = name.LastIndexOf('.');
        return dotIndex >= 0 ? name.Substring(dotIndex + 1) : name;
    }
}
