namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// Compile-time metadata for a single embedded entity property on an HTO,
/// storing the relation names, target entity info, collection flag, and nullability.
/// </summary>
internal readonly record struct EmbeddedEntityMetadata(
    /// <summary>Relation types from <c>[Relations]</c> attribute.</summary>
    EquatableArray<string> Relations,
    /// <summary>Schema name of the target HTO (derived via <c>DeriveSchemaName</c>).</summary>
    string TargetSchemaName,
    /// <summary>Siren classes of the target HTO from <c>[HypermediaObject(Classes)]</c>.</summary>
    EquatableArray<string> TargetClasses,
    /// <summary>Whether this embedded entity represents a collection.</summary>
    bool IsCollection,
    /// <summary>Title from <c>[Title]</c> attribute or XML doc <c>&lt;summary&gt;</c>.</summary>
    string? Title,
    /// <summary>Description from <c>[Description]</c> attribute or XML doc <c>&lt;remarks&gt;</c>.</summary>
    string? Description,
    bool IsDeprecated,
    string? DeprecationMessage,
    /// <summary>Whether the embedded entity property is non-nullable (mandatory).</summary>
    bool IsMandatory,
    /// <summary>Access groups from <c>[HypermediaAccessGroup]</c>. Empty if none.</summary>
    EquatableArray<string> AccessGroups);
