using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// Incremental source generator that analyzes HTO (HypermediaTypedObject) classes
/// and emits <c>GetSchema()</c> methods producing <c>EntityTypeSchema</c> instances,
/// as well as per-assembly schema registries.
/// Pipeline wiring only — analysis lives in <see cref="HtoMetadataExtractor"/> /
/// <see cref="ActionResultMappingExtractor"/>, emission in the <c>*Emitter</c> classes.
/// </summary>
[Generator]
public class HtoSchemaGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Extract [assembly: HypermediaAssembly] configuration from the compilation.
        // Returns null if the attribute is absent (no generation), or the Schema/Siren settings.
        var assemblyConfig = context.CompilationProvider.Select(static (compilation, _) =>
        {
            var attr = compilation.Assembly.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == WellKnownTypeNames.HypermediaAssemblyAttributeFullName);

            if (attr == null)
            {
                return ((bool Schema, bool Siren)?)null;
            }

            var schema = true;
            var siren = false;

            foreach (var named in attr.NamedArguments)
            {
                if (named.Key == "Schema" && named.Value.Value is bool s)
                {
                    schema = s;
                }
                else if (named.Key == "Siren" && named.Value.Value is bool si)
                {
                    siren = si;
                }
            }

            return (schema, siren);
        });

        var htoTypes = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                WellKnownTypeNames.HypermediaObjectAttributeFullName,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => HtoMetadataExtractor.ExtractHtoMetadata(ctx))
            .Where(static m => m.HasValue)
            .Select(static (m, _) => m!.Value);

        // Extract action result mappings from controller [HypermediaActionEndpoint] attributes with ResultType
        var actionResultMappings = context.CompilationProvider.Select(static (compilation, _) =>
            ActionResultMappingExtractor.ExtractActionResultMappings(compilation));

        // Combine each HTO with the assembly configuration and action result mappings
        var htosWithConfig = htoTypes.Combine(assemblyConfig).Combine(actionResultMappings);

        context.RegisterSourceOutput(htosWithConfig, static (spc, combined) =>
        {
            var ((metadata, config), resultData) = combined;
            var resultMappings = resultData.Mappings;

            // Emit warnings for ResultType not being a HypermediaObject
            foreach (var (resultTypeName, htoClassName, actionPropName) in resultData.NotHtoWarnings)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    GeneratorDiagnostics.ResultTypeNotHypermediaObject,
                    Location.None,
                    resultTypeName, htoClassName, actionPropName));
            }

            // Emit warnings for 201 response without ResultType
            foreach (var (controllerName, methodName, actionPropName) in resultData.Missing201Warnings)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    GeneratorDiagnostics.MissingResultTypeWith201,
                    Location.None,
                    controllerName, methodName, actionPropName));
            }

            // No [HypermediaAssembly] attribute — emit nothing
            if (config == null)
            {
                return;
            }

            var schema = config.Value.Schema;
            var siren = config.Value.Siren;

            // Siren = true forces Schema = true
            if (siren && !schema)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    GeneratorDiagnostics.SirenRequiresSchema,
                    Location.None));
                schema = true;
            }

            // Schema = false — emit nothing (safety hatch)
            if (!schema)
            {
                return;
            }

            // Enrich actions with ResultType from controller endpoint attributes
            metadata = ActionResultMappingExtractor.EnrichActionsWithResultMappings(metadata, resultMappings);

            foreach (var propertyName in metadata.EmbeddedEntityPropertiesWithoutRelations)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    GeneratorDiagnostics.EmbeddedEntityMissingRelations,
                    Location.None,
                    propertyName,
                    metadata.ClassName));
            }

            foreach (var propertyName in metadata.LinkPropertiesWithoutRelations)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    GeneratorDiagnostics.LinkMissingRelations,
                    Location.None,
                    propertyName,
                    metadata.ClassName));
            }

            // Detect duplicate link relations
            var seenLinkRelations = new Dictionary<string, string>(); // relKey → first property name
            foreach (var link in metadata.Links)
            {
                var relKey = string.Join(",", link.Relations.OrderBy(r => r, System.StringComparer.Ordinal));
                if (seenLinkRelations.TryGetValue(relKey, out var firstPropertyName))
                {
                    spc.ReportDiagnostic(Diagnostic.Create(
                        GeneratorDiagnostics.DuplicateLinkRelations,
                        Location.None,
                        firstPropertyName,
                        link.PropertyName,
                        metadata.ClassName));
                }
                else
                {
                    seenLinkRelations[relKey] = link.PropertyName;
                }
            }

            spc.AddSource(
                $"{metadata.ClassName}Schema.g.cs",
                SchemaEmitter.GenerateSchemaSource(metadata));

            if (metadata.Properties.Length > 0)
            {
                spc.AddSource(
                    $"{metadata.ClassName}Properties.g.cs",
                    PropertiesPocoEmitter.GeneratePropertiesPoco(metadata));
            }

            // Siren = true — emit ToSiren() and ToSirenEmbedded() extension methods
            if (siren)
            {
                spc.AddSource(
                    $"{metadata.ClassName}SirenExtensions.g.cs",
                    SirenEmitter.GenerateSirenSource(metadata));
            }
        });

        // Collect all HTOs and emit a per-assembly registry + assembly attribute.
        var assemblyName = context.CompilationProvider.Select(
            static (compilation, _) => SanitizeAssemblyName(compilation.AssemblyName ?? "Unknown"));

        var allHtosWithConfig = htoTypes.Collect().Combine(assemblyConfig).Combine(assemblyName).Combine(actionResultMappings);

        context.RegisterSourceOutput(allHtosWithConfig, static (spc, combined) =>
        {
            var (((allHtos, config), assemblyNameSafe), resultData) = combined;

            // No [HypermediaAssembly] or Schema = false — no registry
            if (config == null)
            {
                return;
            }

            var schema = config.Value.Schema;
            var siren = config.Value.Siren;
            if (siren && !schema)
            {
                schema = true;
            }

            if (!schema || allHtos.IsEmpty)
            {
                return;
            }

            spc.AddSource(
                $"HypermediaSchemaRegistry.g.cs",
                RegistryEmitter.GenerateRegistrySource(allHtos, assemblyNameSafe));

            // Siren = true — emit shared SirenHelper class with AddLink, AddAction, etc.
            if (siren)
            {
                spc.AddSource(
                    "SirenHelper.g.cs",
                    SirenHelperEmitter.GenerateSirenHelper());
            }

            // Emit action result registry for multi-assembly support
            if (!resultData.Mappings.IsEmpty)
            {
                spc.AddSource(
                    $"HypermediaActionResultRegistry.g.cs",
                    RegistryEmitter.GenerateActionResultRegistrySource(resultData.Mappings, assemblyNameSafe));
            }
        });
    }

    /// <summary>
    /// Sanitizes an assembly name for use as a C# identifier suffix.
    /// Replaces non-alphanumeric characters with underscores.
    /// </summary>
    internal static string SanitizeAssemblyName(string assemblyName)
    {
        var sb = new StringBuilder(assemblyName.Length);
        foreach (var c in assemblyName)
        {
            sb.Append(char.IsLetterOrDigit(c) ? c : '_');
        }

        return sb.ToString();
    }
}
