using System.Linq;

namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// Compile-time metadata extracted from an HTO class, used by the incremental generator
/// to produce schema and mapper source code.
/// </summary>
/// <param name="Namespace">The namespace of the HTO class.</param>
/// <param name="ClassName">The class name of the HTO.</param>
/// <param name="SchemaName">The schema name derived from the class name.</param>
/// <param name="Title">Title from <c>[HypermediaObject(Title)]</c> or XML doc summary.</param>
/// <param name="Description">Description from <c>[Description]</c> or XML doc remarks.</param>
/// <param name="IsDeprecated">Whether the HTO is marked as deprecated.</param>
/// <param name="DeprecationMessage">Deprecation message, if any.</param>
/// <param name="Classes">Siren classes from <c>[HypermediaObject(Classes)]</c>.</param>
/// <param name="AccessGroups">Access groups from <c>[HypermediaAccessGroup]</c> on the HTO class. Empty if none.</param>
/// <param name="Properties">Data properties metadata.</param>
/// <param name="Links">Link properties metadata.</param>
/// <param name="Actions">Action properties metadata.</param>
/// <param name="EmbeddedEntities">Embedded entity properties metadata.</param>
/// <param name="EmbeddedEntityPropertiesWithoutRelations">Property names of type <c>IEmbeddedEntity</c> but missing <c>[Relations]</c>. Used to emit RY0020 warnings.</param>
/// <param name="LinkPropertiesWithoutRelations">Property names of type <c>ILink</c> but missing <c>[Relations]</c>. Used to emit RY0021 warnings.</param>
/// <param name="InvalidPropertyNameOverrides">Properties whose <c>[HypermediaProperty(Name)]</c> override is not a valid C# identifier. Used to emit RY0022 warnings; the override is ignored.</param>
/// <param name="HasPropertiesTypeCollision">Whether a user-defined type named <c>{ClassName}Properties</c> already exists in the HTO's namespace. Used to emit RY0023 and skip POCO emission.</param>
/// <param name="HasSirenExtensionsTypeCollision">Whether a user-defined type named <c>{ClassName}SirenExtensions</c> already exists in the HTO's namespace. Used to emit RY0023 and skip mapper emission.</param>
internal readonly record struct HtoMetadata(
    string Namespace,
    string ClassName,
    string SchemaName,
    string? Title,
    string? Description,
    bool IsDeprecated,
    string? DeprecationMessage,
    EquatableArray<string> Classes,
    EquatableArray<string> AccessGroups,
    EquatableArray<PropertyMetadata> Properties,
    EquatableArray<LinkMetadata> Links,
    EquatableArray<ActionMetadata> Actions,
    EquatableArray<EmbeddedEntityMetadata> EmbeddedEntities,
    EquatableArray<string> EmbeddedEntityPropertiesWithoutRelations,
    EquatableArray<string> LinkPropertiesWithoutRelations,
    EquatableArray<InvalidPropertyNameOverride> InvalidPropertyNameOverrides,
    bool HasPropertiesTypeCollision,
    bool HasSirenExtensionsTypeCollision)
{
    /// <summary>
    /// The namespace-qualified HTO class name. Disambiguates same-named HTOs in different
    /// namespaces — used for hint names and as the action-result mapping key.
    /// </summary>
    public string FullClassName
        => string.IsNullOrEmpty(Namespace) ? ClassName : Namespace + "." + ClassName;

    /// <summary>
    /// Whether GetSchema() needs an <c>IJsonSchemaFactory</c> parameter
    /// (true when there are data properties or parameterized actions).
    /// </summary>
    public bool NeedsSchemaFactory
        => Properties.Length > 0 || Actions.Any(a => a.ParameterTypeFullName != null);
}
