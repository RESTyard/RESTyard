using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        return ComposeSchema(entityTypes, options, logger);
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
    /// Composes a <see cref="HypermediaApiSchema"/> from a list of entity types and options.
    /// Applies defaults for unset option fields (title from entry assembly, version, entry point detection).
    /// </summary>
    public static HypermediaApiSchema ComposeSchema(
        List<EntityTypeSchema> entityTypes,
        HypermediaSchemaOptions? options,
        ILogger? logger)
    {
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

        return new HypermediaApiSchema
        {
            SchemaVersion = SchemaVersion,
            Title = title,
            Description = options?.Description,
            ApiVersion = apiVersion,
            EntryPointName = entryPointName ?? string.Empty,
            ExternalDocsUrl = options?.ExternalDocsUrl,
            EntityTypes = entityTypes,
        };
    }

    private static string? DetectEntryPoint(List<EntityTypeSchema> entityTypes)
    {
        return entityTypes
            .FirstOrDefault(e => e.Classes.Any(c =>
                string.Equals(c, "EntryPoint", StringComparison.OrdinalIgnoreCase)))
            ?.Name;
    }
}
