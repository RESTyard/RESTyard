using Microsoft.CodeAnalysis;

namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// All diagnostic descriptors reported by the HTO schema generator, in one place
/// so IDs, severities, and wording can be checked for consistency.
/// </summary>
internal static class GeneratorDiagnostics
{
    // Severity note (GEN-12): diagnostics whose message says the generator recovered
    // ("will be ignored", "has been forced to true") are warnings — the build succeeds and
    // the message describes what was done. Errors are reserved for cases where generation
    // (or the subsequent compilation) cannot proceed correctly (RY0023).
    internal static readonly DiagnosticDescriptor SirenRequiresSchema = new(
        id: "RY0030",
        title: "Siren = true requires Schema generation",
        messageFormat: "[HypermediaAssembly] has Siren = true but Schema = false — Schema has been forced to true because Siren mappers depend on the generated properties POCOs",
        category: "RESTyard.Schema",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    internal static readonly DiagnosticDescriptor MissingResultTypeWith201 = new(
        id: "RY0031",
        title: "Action endpoint returns 201 but has no ResultType",
        messageFormat: "Action endpoint '{0}.{1}' on '{2}' has a 201 response annotation but no ResultType — consider adding ResultType to declare the result entity for schema generation",
        category: "RESTyard.Schema",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    internal static readonly DiagnosticDescriptor ResultTypeNotHypermediaObject = new(
        id: "RY0032",
        title: "ResultType is not a HypermediaObject",
        messageFormat: "ResultType '{0}' on action endpoint for '{1}.{2}' is not decorated with [HypermediaObject] — schema cannot describe the result entity. If this action returns a non-hypermedia resource, consider removing ResultType or suppress this warning.",
        category: "RESTyard.Schema",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    internal static readonly DiagnosticDescriptor EmbeddedEntityMissingRelations = new(
        id: "RY0020",
        title: "Embedded entity property missing [Relations] attribute",
        messageFormat: "Property '{0}' on '{1}' is of type IEmbeddedEntity but has no [Relations] attribute — it will be ignored in the schema",
        category: "RESTyard.Schema",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    internal static readonly DiagnosticDescriptor LinkMissingRelations = new(
        id: "RY0021",
        title: "Link property missing [Relations] attribute",
        messageFormat: "Property '{0}' on '{1}' is of type ILink but has no [Relations] attribute — it will be ignored in the schema",
        category: "RESTyard.Schema",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    internal static readonly DiagnosticDescriptor InvalidPropertyNameOverride = new(
        id: "RY0022",
        title: "Property name override is not a valid identifier",
        messageFormat: "[HypermediaProperty(Name = \"{0}\")] on property '{1}' of '{2}' is not a valid C# identifier — the generated properties POCO uses the name structurally, so the override is ignored and the property name '{1}' is used instead",
        category: "RESTyard.Schema",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    internal static readonly DiagnosticDescriptor GeneratedTypeNameCollision = new(
        id: "RY0023",
        title: "Existing type collides with a generated type",
        messageFormat: "A type named '{0}' is already defined in this assembly and collides with {1} the schema generator emits — rename the existing type",
        category: "RESTyard.Schema",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    internal static readonly DiagnosticDescriptor DuplicateLinkRelations = new(
        id: "RY0040",
        title: "Duplicate link relations",
        messageFormat: "Properties '{0}' and '{1}' on '{2}' have identical [Relations] — the last one will win at runtime (Siren relations identify a unique link)",
        category: "RESTyard.Siren",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    // Info, not Warning: multiple embedded entities sharing relations are valid Siren and
    // intentionally allowed at runtime — this is only a hint against copy-paste mistakes.
    internal static readonly DiagnosticDescriptor DuplicateEmbeddedEntityRelations = new(
        id: "RY0041",
        title: "Duplicate embedded entity relations",
        messageFormat: "Properties '{0}' and '{1}' on '{2}' have identical [Relations] — multiple embedded entities with the same relations are valid, but verify this is intentional",
        category: "RESTyard.Siren",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);
}
