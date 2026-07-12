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
                // Records are ordinary classes at the symbol level — positional properties are
                // public instance properties, and the compiler-generated EqualityContract is
                // protected, so the extractor's accessibility filter already excludes it.
                predicate: static (node, _) => node is ClassDeclarationSyntax or RecordDeclarationSyntax,
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

        // [HypermediaObjectEndpoint<THto>] applications — tracked only for duplicate-endpoint
        // detection (RY0033, GEN-11); object endpoints carry no schema data of their own.
        var objectEndpointOccurrences = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                WellKnownTypeNames.HypermediaObjectEndpointAttributeMetadataName,
                predicate: static (node, _) => node is MethodDeclarationSyntax,
                transform: static (ctx, _) => ActionResultMappingExtractor.ExtractObjectEndpointOccurrences(ctx))
            .WithTrackingName(TrackingNames.ObjectEndpointOccurrences);

        // Compilation-level diagnostics: reported once, not per HTO, and also in assemblies
        // without any HTO (e.g. controller-only assemblies). No [HypermediaAssembly] → nothing.
        context.RegisterSourceOutput(
            actionResultMappings.Combine(assemblyConfig).Combine(objectEndpointOccurrences.Collect()),
            static (spc, combined) =>
        {
            var ((resultData, config), objectOccurrences) = combined;

            if (config == null)
            {
                return;
            }

            var (effectiveConfig, warnSirenRequiresSchema) = config.Value.Normalize();
            if (warnSirenRequiresSchema)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    GeneratorDiagnostics.SirenRequiresSchema,
                    effectiveConfig.AttributeLocation?.ToLocation()));
            }

            if (!effectiveConfig.Schema)
            {
                return;
            }

            foreach (var warning in resultData.NotHtoWarnings)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    GeneratorDiagnostics.ResultTypeNotHypermediaObject,
                    warning.Location?.ToLocation(),
                    warning.ResultTypeName, warning.HtoClassName, warning.ActionPropertyName));
            }

            foreach (var warning in resultData.Missing201Warnings)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    GeneratorDiagnostics.MissingResultTypeWith201,
                    warning.Location?.ToLocation(),
                    warning.ControllerName, warning.MethodName, warning.ActionPropertyName));
            }

            // GEN-11: multiple endpoints for the same HTO/action — one RY0033 per surplus
            // attribute application (every occurrence after the first, in file order).
            var allOccurrences = resultData.EndpointOccurrences
                .Concat(objectOccurrences.SelectMany(static o => o));
            foreach (var duplicate in ActionResultMappingExtractor.FindDuplicateEndpoints(allOccurrences))
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    GeneratorDiagnostics.MultipleEndpoints,
                    duplicate.Location?.ToLocation(),
                    duplicate.DisplayName));
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

            foreach (var property in metadata.EmbeddedEntityPropertiesWithoutRelations)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    GeneratorDiagnostics.EmbeddedEntityMissingRelations,
                    property.Location?.ToLocation(),
                    property.PropertyName,
                    metadata.ClassName));
            }

            foreach (var property in metadata.LinkPropertiesWithoutRelations)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    GeneratorDiagnostics.LinkMissingRelations,
                    property.Location?.ToLocation(),
                    property.PropertyName,
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
                        link.Location?.ToLocation(),
                        firstPropertyName,
                        link.PropertyName,
                        metadata.ClassName));
                }
                else
                {
                    seenLinkRelations[relKey] = link.PropertyName;
                }
            }

            // Duplicate embedded entity relations are valid at runtime — RY0041 is only a hint.
            var seenEmbeddedRelations = new Dictionary<string, string>(); // relKey → first property name
            foreach (var embedded in metadata.EmbeddedEntities)
            {
                var relKey = string.Join(",", embedded.Relations.OrderBy(r => r, System.StringComparer.Ordinal));
                if (seenEmbeddedRelations.TryGetValue(relKey, out var firstPropertyName))
                {
                    spc.ReportDiagnostic(Diagnostic.Create(
                        GeneratorDiagnostics.DuplicateEmbeddedEntityRelations,
                        embedded.Location?.ToLocation(),
                        firstPropertyName,
                        embedded.PropertyName,
                        metadata.ClassName));
                }
                else
                {
                    seenEmbeddedRelations[relKey] = embedded.PropertyName;
                }
            }

            foreach (var invalidName in metadata.InvalidPropertyNameOverrides)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    GeneratorDiagnostics.InvalidPropertyNameOverride,
                    invalidName.Location?.ToLocation(),
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
                    metadata.PropertiesTypeCollision?.ToLocation(),
                    $"{metadata.FullClassName}Properties",
                    $"the data-properties POCO for '{metadata.ClassName}'"));
            }

            var sirenExtensionsCollision = siren && metadata.HasSirenExtensionsTypeCollision;
            if (sirenExtensionsCollision)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    GeneratorDiagnostics.GeneratedTypeNameCollision,
                    metadata.SirenExtensionsTypeCollision?.ToLocation(),
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
        // Null = no collision; otherwise the colliding type's location for RY0023.
        var sirenHelperCollision = context.CompilationProvider
            .Select(static (compilation, _) =>
                compilation.Assembly.GetTypeByMetadataName("SirenHelper") is { } sirenHelperType
                    ? LocationInfo.FromSymbol(sirenHelperType)
                    : null)
            .WithTrackingName(TrackingNames.SirenHelperCollision);

        var allHtosWithConfig = htoTypes.Collect().Combine(assemblyConfig).Combine(assemblyName)
            .Combine(actionResultMappings).Combine(sirenHelperCollision);

        context.RegisterSourceOutput(allHtosWithConfig, static (spc, combined) =>
        {
            var ((((allHtos, config), assemblyNameSafe), resultData), sirenHelperCollisionLocation) = combined;

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

            // Duplicate schema names ("HypermediaCustomerHto" and "CustomerHto" both derive
            // "Customer") make cross-references ambiguous — RY0024 error per colliding HTO,
            // ordered by class name so the reported pairs are deterministic.
            var seenSchemaNames = new Dictionary<string, HtoMetadata>();
            foreach (var hto in allHtos.OrderBy(h => h.FullClassName, System.StringComparer.Ordinal))
            {
                if (seenSchemaNames.TryGetValue(hto.SchemaName, out var first))
                {
                    spc.ReportDiagnostic(Diagnostic.Create(
                        GeneratorDiagnostics.DuplicateSchemaName,
                        hto.Location?.ToLocation(),
                        first.FullClassName,
                        hto.FullClassName,
                        hto.SchemaName));
                }
                else
                {
                    seenSchemaNames[hto.SchemaName] = hto;
                }
            }

            var siren = effectiveConfig.Siren;

            spc.AddSource(
                $"HypermediaSchemaRegistry.g.cs",
                RegistryEmitter.GenerateRegistrySource(allHtos, assemblyNameSafe));

            // Siren = true — emit shared SirenHelper class with AddLink, AddAction, etc.
            if (siren)
            {
                if (sirenHelperCollisionLocation is { } collisionLocation)
                {
                    spc.ReportDiagnostic(Diagnostic.Create(
                        GeneratorDiagnostics.GeneratedTypeNameCollision,
                        collisionLocation.ToLocation(),
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
    /// Distinct assembly names can sanitize to the same identifier ("My.App"/"My_App") — this is
    /// acceptable: the registry types live in different assemblies and are discovered via assembly
    /// attributes, never referenced by name across assemblies, so equal type names cannot clash.
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
