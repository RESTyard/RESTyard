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
        var assemblyConfig = context.CompilationProvider
            .Select(static (compilation, _) => AssemblyConfig.FromCompilation(compilation))
            .WithTrackingName(TrackingNames.AssemblyConfig);

        var htoTypes = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                WellKnownTypeNames.HypermediaObjectAttributeFullName,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => HtoMetadataExtractor.ExtractHtoMetadata(ctx))
            .Where(static m => m.HasValue)
            .Select(static (m, _) => m!.Value)
            .WithTrackingName(TrackingNames.HtoTypes);

        // Extract action result mappings from controller [HypermediaActionEndpoint<THto>] attributes
        // with ResultType — incrementally, per attributed method (GEN-04).
        var endpointResultMappings = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                WellKnownTypeNames.HypermediaActionEndpointAttributeMetadataName,
                predicate: static (node, _) => node is MethodDeclarationSyntax,
                transform: static (ctx, _) => ActionResultMappingExtractor.ExtractFromEndpointAttributes(ctx))
            .WithTrackingName(TrackingNames.EndpointResultMappings);

        // Legacy HttpMethodHypermediaAction-derived attributes are matched by base type, which
        // ForAttributeWithMetadataName cannot express — scan the source assembly per compilation.
        // The scan re-runs on every edit, but its output is equatable so downstream caching survives.
        var legacyResultMappings = context.CompilationProvider
            .Select(static (compilation, _) =>
                ActionResultMappingExtractor.ExtractLegacyActionResults(compilation))
            .WithTrackingName(TrackingNames.LegacyResultMappings);

        var actionResultMappings = endpointResultMappings.Collect()
            .Combine(legacyResultMappings)
            .Select(static (combined, _) => ActionResultMappingExtractor.Merge(combined.Left, combined.Right))
            .WithTrackingName(TrackingNames.ActionResultMappings);

        // Compilation-level diagnostics: reported once, not per HTO, and also in assemblies
        // without any HTO (e.g. controller-only assemblies). No [HypermediaAssembly] → nothing.
        context.RegisterSourceOutput(actionResultMappings.Combine(assemblyConfig), static (spc, combined) =>
        {
            var (resultData, config) = combined;

            if (config == null)
            {
                return;
            }

            var (effectiveConfig, warnSirenRequiresSchema) = config.Value.Normalize();
            if (warnSirenRequiresSchema)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    GeneratorDiagnostics.SirenRequiresSchema,
                    Location.None));
            }

            if (!effectiveConfig.Schema)
            {
                return;
            }

            foreach (var warning in resultData.NotHtoWarnings)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    GeneratorDiagnostics.ResultTypeNotHypermediaObject,
                    Location.None,
                    warning.ResultTypeName, warning.HtoClassName, warning.ActionPropertyName));
            }

            foreach (var warning in resultData.Missing201Warnings)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    GeneratorDiagnostics.MissingResultTypeWith201,
                    Location.None,
                    warning.ControllerName, warning.MethodName, warning.ActionPropertyName));
            }
        });

        // Combine each HTO with the assembly configuration and action result mappings
        var htosWithConfig = htoTypes.Combine(assemblyConfig).Combine(actionResultMappings);

        context.RegisterSourceOutput(htosWithConfig, static (spc, combined) =>
        {
            var ((metadata, config), resultData) = combined;
            var resultMappings = resultData.Mappings;

            // No [HypermediaAssembly] attribute — emit nothing
            if (config == null)
            {
                return;
            }

            // RY0030 for the Siren-forces-Schema override is reported once in the
            // compilation-level diagnostics output above.
            var (effectiveConfig, _) = config.Value.Normalize();

            // Schema = false — emit nothing (safety hatch)
            if (!effectiveConfig.Schema)
            {
                return;
            }

            var siren = effectiveConfig.Siren;

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

            foreach (var invalidName in metadata.InvalidPropertyNameOverrides)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    GeneratorDiagnostics.InvalidPropertyNameOverride,
                    Location.None,
                    invalidName.InvalidName,
                    invalidName.PropertyName,
                    metadata.ClassName));
            }

            // A user-defined type with a generated type's name would produce a CS0101
            // duplicate-definition error on generated code — report RY0023 with the real
            // cause instead and skip the colliding artifact.
            var propertiesCollision = metadata.Properties.Length > 0 && metadata.HasPropertiesTypeCollision;
            if (propertiesCollision)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    GeneratorDiagnostics.GeneratedTypeNameCollision,
                    Location.None,
                    $"{metadata.FullClassName}Properties",
                    $"the data-properties POCO for '{metadata.ClassName}'"));
            }

            var sirenExtensionsCollision = siren && metadata.HasSirenExtensionsTypeCollision;
            if (sirenExtensionsCollision)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    GeneratorDiagnostics.GeneratedTypeNameCollision,
                    Location.None,
                    $"{metadata.FullClassName}SirenExtensions",
                    $"the Siren mapper for '{metadata.ClassName}'"));
            }

            // Hint names are namespace-qualified — same-named HTOs in different namespaces
            // would otherwise produce duplicate hint names and crash the whole generation.
            spc.AddSource(
                $"{metadata.FullClassName}Schema.g.cs",
                SchemaEmitter.GenerateSchemaSource(metadata));

            if (metadata.Properties.Length > 0 && !propertiesCollision)
            {
                spc.AddSource(
                    $"{metadata.FullClassName}Properties.g.cs",
                    PropertiesPocoEmitter.GeneratePropertiesPoco(metadata));
            }

            // Siren = true — emit ToSiren() and ToSirenEmbedded() extension methods.
            // Skipped on collisions: the mapper itself would collide, or it would bind
            // against the user's Properties type instead of the (skipped) generated POCO.
            if (siren && !sirenExtensionsCollision && !propertiesCollision)
            {
                spc.AddSource(
                    $"{metadata.FullClassName}SirenExtensions.g.cs",
                    SirenEmitter.GenerateSirenSource(metadata));
            }
        });

        // Collect all HTOs and emit a per-assembly registry + assembly attribute.
        var assemblyName = context.CompilationProvider
            .Select(static (compilation, _) => SanitizeAssemblyName(compilation.AssemblyName ?? "Unknown"))
            .WithTrackingName(TrackingNames.AssemblyName);

        // A user-defined global-namespace SirenHelper type would collide with the generated one.
        var sirenHelperCollision = context.CompilationProvider
            .Select(static (compilation, _) => compilation.Assembly.GetTypeByMetadataName("SirenHelper") != null)
            .WithTrackingName(TrackingNames.SirenHelperCollision);

        var allHtosWithConfig = htoTypes.Collect().Combine(assemblyConfig).Combine(assemblyName)
            .Combine(actionResultMappings).Combine(sirenHelperCollision);

        context.RegisterSourceOutput(allHtosWithConfig, static (spc, combined) =>
        {
            var ((((allHtos, config), assemblyNameSafe), resultData), hasSirenHelperCollision) = combined;

            // No [HypermediaAssembly] or Schema = false — no registry.
            // RY0030 for the Siren-forces-Schema override is reported in the
            // compilation-level diagnostics output.
            if (config == null)
            {
                return;
            }

            var (effectiveConfig, _) = config.Value.Normalize();
            if (!effectiveConfig.Schema)
            {
                return;
            }

            // Action result registry for multi-assembly support — emitted before the HTO check
            // because its main use case is controller-only assemblies with zero HTOs (the HTOs
            // live in a referenced assembly and are enriched at runtime).
            if (!resultData.Mappings.IsEmpty)
            {
                spc.AddSource(
                    $"HypermediaActionResultRegistry.g.cs",
                    RegistryEmitter.GenerateActionResultRegistrySource(resultData.Mappings, assemblyNameSafe));
            }

            if (allHtos.IsEmpty)
            {
                return;
            }

            var siren = effectiveConfig.Siren;

            spc.AddSource(
                $"HypermediaSchemaRegistry.g.cs",
                RegistryEmitter.GenerateRegistrySource(allHtos, assemblyNameSafe));

            // Siren = true — emit shared SirenHelper class with AddLink, AddAction, etc.
            if (siren)
            {
                if (hasSirenHelperCollision)
                {
                    spc.ReportDiagnostic(Diagnostic.Create(
                        GeneratorDiagnostics.GeneratedTypeNameCollision,
                        Location.None,
                        "SirenHelper",
                        "the shared Siren helper emitted for Siren = true"));
                }
                else
                {
                    spc.AddSource(
                        "SirenHelper.g.cs",
                        SirenHelperEmitter.GenerateSirenHelper());
                }
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
