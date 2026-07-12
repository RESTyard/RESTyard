using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RESTyard.Schema.Model;
using RESTyard.Schema.SchemaGeneration;

namespace RESTyard.Schema;

/// <summary>
/// Builds a <see cref="HypermediaApiSchema"/> by discovering per-assembly schema registries
/// and aggregating them with configured options.
/// </summary>
public static class HypermediaSchemaBuilder
{
    private const string SchemaVersion = "1.0.0";

    /// <summary>
    /// Builds a <see cref="HypermediaApiSchema"/> by scanning loaded assemblies for
    /// <see cref="HypermediaSchemaRegistryAttribute"/> and invoking each registry's
    /// <c>GetSchemas(IJsonSchemaFactory)</c> method.
    /// </summary>
    /// <param name="factory">The JSON schema factory used by registries to generate property/parameter schemas.</param>
    /// <param name="options">
    /// Schema metadata options. When null, all defaults are applied.
    /// When provided, individual null fields still receive defaults.
    /// </param>
    /// <param name="logger">Optional logger for diagnostics (registry discovery warnings, errors).</param>
    /// <returns>The aggregated <see cref="HypermediaApiSchema"/>.</returns>
    public static HypermediaApiSchema Build(
        IJsonSchemaFactory factory,
        HypermediaSchemaOptions? options = null,
        ILogger? logger = null)
    {
        var entityTypes = DiscoverEntityTypes(factory, logger);
        var actionResultMappings = DiscoverActionResultMappings(logger);
        return ComposeSchema(entityTypes, actionResultMappings, options, logger);
    }

    /// <summary>
    /// Scans all loaded assemblies for <see cref="HypermediaSchemaRegistryAttribute"/> and invokes
    /// each registry's <c>GetSchemas</c> method to collect all <see cref="EntityTypeSchema"/> instances.
    /// </summary>
    public static List<EntityTypeSchema> DiscoverEntityTypes(IJsonSchemaFactory factory, ILogger? logger = null)
    {
        var allEntityTypes = new List<EntityTypeSchema>();

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var registriesFound = 0;

        foreach (var assembly in assemblies)
        {
            HypermediaSchemaRegistryAttribute? attr;
            try
            {
                attr = assembly.GetCustomAttribute<HypermediaSchemaRegistryAttribute>();
            }
            catch
            {
                // Some dynamic assemblies throw on GetCustomAttribute — skip them
                continue;
            }

            if (attr == null)
            {
                continue;
            }

            registriesFound++;
            var registryType = attr.RegistryType;

            var getSchemas = registryType.GetMethod("GetSchemas", BindingFlags.Public | BindingFlags.Static);
            if (getSchemas == null)
            {
                logger?.LogWarning(
                    "Schema registry type '{RegistryType}' in assembly '{Assembly}' has no public static GetSchemas method",
                    registryType.FullName, assembly.GetName().Name);
                continue;
            }

            try
            {
                var result = getSchemas.Invoke(null, [factory]);
                if (result is IReadOnlyList<EntityTypeSchema> schemas)
                {
                    allEntityTypes.AddRange(schemas);
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex,
                    "Failed to invoke GetSchemas on registry type '{RegistryType}' in assembly '{Assembly}'",
                    registryType.FullName, assembly.GetName().Name);
            }
        }

        if (registriesFound == 0)
        {
            logger?.LogWarning(
                "No HypermediaSchemaRegistry found in loaded assemblies. " +
                "Ensure [assembly: HypermediaAssembly] is present in assemblies containing HTOs " +
                "and that Schema is not set to false.");
        }

        return allEntityTypes;
    }

    /// <summary>
    /// Scans all loaded assemblies for <see cref="HypermediaActionResultRegistryAttribute"/> and invokes
    /// each registry's <c>GetMappings</c> method to collect all <see cref="ActionResultMapping"/> instances.
    /// These come from assemblies whose controllers declare <c>ResultType</c> for actions on HTOs
    /// defined in a different assembly.
    /// </summary>
    public static List<ActionResultMapping> DiscoverActionResultMappings(ILogger? logger = null)
    {
        var allMappings = new List<ActionResultMapping>();

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            HypermediaActionResultRegistryAttribute? attr;
            try
            {
                attr = assembly.GetCustomAttribute<HypermediaActionResultRegistryAttribute>();
            }
            catch
            {
                // Some dynamic assemblies throw on GetCustomAttribute — skip them
                continue;
            }

            if (attr == null)
            {
                continue;
            }

            var registryType = attr.RegistryType;

            var getMappings = registryType.GetMethod("GetMappings", BindingFlags.Public | BindingFlags.Static);
            if (getMappings == null)
            {
                logger?.LogWarning(
                    "Action result registry type '{RegistryType}' in assembly '{Assembly}' has no public static GetMappings method",
                    registryType.FullName, assembly.GetName().Name);
                continue;
            }

            try
            {
                var result = getMappings.Invoke(null, []);
                if (result is IReadOnlyList<ActionResultMapping> mappings)
                {
                    allMappings.AddRange(mappings);
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex,
                    "Failed to invoke GetMappings on action result registry type '{RegistryType}' in assembly '{Assembly}'",
                    registryType.FullName, assembly.GetName().Name);
            }
        }

        return allMappings;
    }

    /// <summary>
    /// Composes a <see cref="HypermediaApiSchema"/> from a list of entity types and options.
    /// Applies defaults for unset option fields (title from entry assembly, version, entry point detection).
    /// </summary>
    public static HypermediaApiSchema ComposeSchema(
        List<EntityTypeSchema> entityTypes,
        HypermediaSchemaOptions? options,
        ILogger? logger)
        => ComposeSchema(entityTypes, [], options, logger);

    /// <summary>
    /// Composes a <see cref="HypermediaApiSchema"/> from a list of entity types and options,
    /// first merging discovered <see cref="ActionResultMapping"/>s into the matching
    /// <see cref="ActionDescription"/>s (multi-assembly <c>ResultType</c> support).
    /// </summary>
    public static HypermediaApiSchema ComposeSchema(
        List<EntityTypeSchema> entityTypes,
        IReadOnlyList<ActionResultMapping> actionResultMappings,
        HypermediaSchemaOptions? options,
        ILogger? logger)
    {
        ApplyActionResultMappings(entityTypes, actionResultMappings, logger);
        ValidateReferences(entityTypes, options?.AllowUnresolvedReferences ?? false, logger);
        var entryAssembly = Assembly.GetEntryAssembly();

        var title = options?.Title
                    ?? entryAssembly?.GetName().Name;

        var apiVersion = options?.ApiVersion;

        var entryPointName = options?.EntryPointName
                             ?? DetectEntryPoint(entityTypes);

        if (entryPointName == null)
        {
            logger?.LogDebug("No entry point entity type detected. Set HypermediaSchemaOptions.EntryPointName explicitly.");
        }

        var definitions = ExtractDefinitions(entityTypes, logger);
        var declaredAccessGroups = CollectDeclaredAccessGroups(entityTypes);

        return new HypermediaApiSchema
        {
            SchemaVersion = SchemaVersion,
            Title = title,
            Description = options?.Description,
            ApiVersion = apiVersion,
            EntryPointName = entryPointName ?? string.Empty,
            ExternalDocsUrl = options?.ExternalDocsUrl,
            EntityTypes = entityTypes,
            Definitions = definitions,
            DeclaredAccessGroups = declaredAccessGroups,
        };
    }

    /// <summary>
    /// Fills <see cref="ActionDescription.ResultName"/>/<see cref="ActionDescription.ResultClasses"/>
    /// from mappings collected in other assemblies. Result information already present (from
    /// compile-time enrichment in the HTO's own assembly) is left untouched.
    /// </summary>
    private static void ApplyActionResultMappings(
        List<EntityTypeSchema> entityTypes,
        IReadOnlyList<ActionResultMapping> mappings,
        ILogger? logger)
    {
        foreach (var mapping in mappings)
        {
            var action = entityTypes
                .FirstOrDefault(e => e.Name == mapping.EntityName)
                ?.Actions.FirstOrDefault(a => a.Name == mapping.ActionName);

            if (action == null)
            {
                logger?.LogWarning(
                    "Action result mapping targets '{EntityName}.{ActionName}' but no such entity type action was discovered",
                    mapping.EntityName, mapping.ActionName);
                continue;
            }

            if (action.ResultName != null)
            {
                continue;
            }

            action.ResultName = mapping.ResultName;
            action.ResultClasses = mapping.ResultClasses;
        }
    }

    /// <summary>
    /// Validates that all cross-references — link and embedded-entity <c>TargetName</c>, action
    /// <c>ResultName</c> — resolve to an existing <see cref="EntityTypeSchema.Name"/>. Dangling
    /// references indicate a missing or unregistered HTO and are logged as warnings. With
    /// <see cref="HypermediaSchemaOptions.AllowUnresolvedReferences"/> enabled, each unresolved
    /// name additionally gets a placeholder entity type (no properties, links, or actions) so
    /// the schema endpoint and diagram mappers remain functional while the API is being built.
    /// External links (no target entity type) are exempt.
    /// </summary>
    private static void ValidateReferences(
        List<EntityTypeSchema> entityTypes,
        bool allowUnresolvedReferences,
        ILogger? logger)
    {
        var knownNames = new HashSet<string>(entityTypes.Select(e => e.Name), StringComparer.Ordinal);
        // Sorted so warnings and placeholder ordering are deterministic across runs
        var danglingReferences = new SortedDictionary<string, List<string>>(StringComparer.Ordinal);

        foreach (var entity in entityTypes)
        {
            foreach (var link in entity.Links)
            {
                if (!link.IsExternal && !string.IsNullOrEmpty(link.TargetName))
                {
                    AddIfDangling(link.TargetName!, $"{entity.Name} link [{string.Join(", ", link.Relations)}]");
                }
            }

            foreach (var embedded in entity.EmbeddedEntities)
            {
                if (!string.IsNullOrEmpty(embedded.TargetName))
                {
                    AddIfDangling(embedded.TargetName, $"{entity.Name} embedded [{string.Join(", ", embedded.Relations)}]");
                }
            }

            foreach (var action in entity.Actions)
            {
                if (!string.IsNullOrEmpty(action.ResultName))
                {
                    AddIfDangling(action.ResultName!, $"{entity.Name}.{action.Name} result");
                }
            }
        }

        foreach (var dangling in danglingReferences)
        {
            logger?.LogWarning(
                "Schema reference '{TargetName}' (referenced by: {Sources}) does not resolve to any " +
                "entity type — the referenced HTO is missing, its assembly is not loaded, or its " +
                "schema generation is disabled.{Placeholder}",
                dangling.Key,
                string.Join("; ", dangling.Value),
                allowUnresolvedReferences ? " A placeholder entity type was added." : string.Empty);

            if (allowUnresolvedReferences)
            {
                entityTypes.Add(new EntityTypeSchema { Name = dangling.Key });
            }
        }

        void AddIfDangling(string targetName, string source)
        {
            if (knownNames.Contains(targetName))
            {
                return;
            }

            if (!danglingReferences.TryGetValue(targetName, out var sources))
            {
                sources = new List<string>();
                danglingReferences[targetName] = sources;
            }

            sources.Add(source);
        }
    }

    /// <summary>
    /// Scans all entity PropertiesSchema and action ParameterSchema for local $defs entries
    /// and extracts them into a deduplicated Definitions dictionary.
    /// Same name + same content → merge. Same name + different content → disambiguate with warning.
    /// </summary>
    private static Dictionary<string, JsonDocument> ExtractDefinitions(
        List<EntityTypeSchema> entityTypes, ILogger? logger)
    {
        var definitions = new Dictionary<string, JsonDocument>();

        foreach (var entity in entityTypes)
        {
            CollectDefsFromSchema(entity.PropertiesSchema, entity.Name, definitions, logger);
            foreach (var action in entity.Actions)
            {
                CollectDefsFromSchema(action.ParameterSchema, $"{entity.Name}.{action.Name}", definitions, logger);
            }
        }

        return definitions;
    }

    private static void CollectDefsFromSchema(
        JsonDocument? schema, string sourceName,
        Dictionary<string, JsonDocument> definitions, ILogger? logger)
    {
        if (schema == null) return;

        var jsonSchema = schema.ToJsonSchema();
        var defs = jsonSchema.GetDefs();
        if (defs == null) return;

        foreach (var kvp in defs)
        {
            // Use the $defs key as the Definitions key — this matches the $ref paths
            // used by mappers. The clean display name is derived at rendering time from $id.
            var name = kvp.Key;
            var defJson = JsonSerializer.SerializeToDocument(kvp.Value);

            if (definitions.TryGetValue(name, out var existing))
            {
                // Same name — check if same content
                var existingJson = existing.RootElement.GetRawText();
                var newJson = defJson.RootElement.GetRawText();
                if (existingJson != newJson)
                {
                    logger?.LogWarning(
                        "Definition '{Name}' found with different schemas in '{Source}' — " +
                        "keeping first occurrence. Consider using fully qualified type names to disambiguate.",
                        name, sourceName);
                }
                // Same content or collision — keep first occurrence
            }
            else
            {
                definitions[name] = defJson;
            }
        }
    }

    /// <summary>
    /// Extracts the CLR type name from a definition schema's $id URI (e.g., "type:MyApp.Address" → "MyApp.Address").
    /// Returns null if no $id is present.
    /// </summary>
    private static string? ExtractTypeNameFromId(Json.Schema.JsonSchema defSchema)
    {
        var id = defSchema.Keywords?.OfType<Json.Schema.IdKeyword>().FirstOrDefault()?.Id;
        if (id == null) return null;

        var idString = id.OriginalString;
        const string prefix = "type:";
        return idString.StartsWith(prefix) ? idString.Substring(prefix.Length) : idString;
    }

    /// <summary>
    /// Collects all unique access group names from entity types, actions, links, and embedded entities.
    /// Returns null when no access groups are declared (keeps JSON clean).
    /// </summary>
    private static IReadOnlyList<string>? CollectDeclaredAccessGroups(List<EntityTypeSchema> entityTypes)
    {
        var groups = new SortedSet<string>(StringComparer.Ordinal);

        foreach (var entity in entityTypes)
        {
            AddGroups(groups, entity.AccessGroups);

            foreach (var action in entity.Actions)
            {
                AddGroups(groups, action.AccessGroups);
            }

            foreach (var link in entity.Links)
            {
                AddGroups(groups, link.AccessGroups);
            }

            foreach (var embedded in entity.EmbeddedEntities)
            {
                AddGroups(groups, embedded.AccessGroups);
            }
        }

        return groups.Count > 0 ? groups.ToList() : null;

        static void AddGroups(SortedSet<string> target, IReadOnlyList<string>? source)
        {
            if (source == null) return;
            foreach (var group in source)
            {
                target.Add(group);
            }
        }
    }

    private static string? DetectEntryPoint(List<EntityTypeSchema> entityTypes)
    {
        return entityTypes
            .FirstOrDefault(e => e.Classes.Any(c =>
                string.Equals(c, "EntryPoint", StringComparison.OrdinalIgnoreCase)))
            ?.Name;
    }
}
