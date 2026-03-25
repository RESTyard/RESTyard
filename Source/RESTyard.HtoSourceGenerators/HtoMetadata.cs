using System.Linq;

namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// Compile-time metadata extracted from an HTO class, used by the incremental generator
/// to produce schema and mapper source code.
/// </summary>
internal readonly record struct HtoMetadata(
    string Namespace,
    string ClassName,
    string SchemaName,
    string? Title,
    string? Description,
    bool IsDeprecated,
    string? DeprecationMessage,
    EquatableArray<string> Classes,
    /// <summary>Access groups from <c>[HypermediaAccessGroup]</c> on the HTO class. Empty if none.</summary>
    EquatableArray<string> AccessGroups,
    EquatableArray<PropertyMetadata> Properties,
    EquatableArray<LinkMetadata> Links,
    EquatableArray<ActionMetadata> Actions,
    EquatableArray<EmbeddedEntityMetadata> EmbeddedEntities,
    /// <summary>
    /// Property names that are of type <c>IEmbeddedEntity</c> but missing <c>[Relations]</c>.
    /// Used to emit RY0020 warnings during source generation.
    /// </summary>
    EquatableArray<string> EmbeddedEntityPropertiesWithoutRelations,
    /// <summary>
    /// Property names that are of type <c>ILink</c> but missing <c>[Relations]</c>.
    /// Used to emit RY0021 warnings during source generation.
    /// </summary>
    EquatableArray<string> LinkPropertiesWithoutRelations)
{
    /// <summary>
    /// Whether GetSchema() needs an <c>IJsonSchemaFactory</c> parameter
    /// (true when there are data properties or parameterized actions).
    /// </summary>
    public bool NeedsSchemaFactory
        => Properties.Length > 0 || Actions.Any(a => a.ParameterTypeFullName != null);
}
