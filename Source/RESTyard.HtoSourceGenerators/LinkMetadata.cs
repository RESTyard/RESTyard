namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// Compile-time metadata for a single link property on an HTO,
/// storing the relation names, target entity info, and nullability.
/// </summary>
/// <param name="PropertyName">The C# property name on the HTO class, used to emit <c>hto.PropertyName</c>.</param>
/// <param name="Relations">Relation types from <c>[Relations]</c> attribute.</param>
/// <param name="TargetSchemaName">Schema name of the target HTO (derived via <c>DeriveSchemaName</c>).</param>
/// <param name="TargetClasses">Siren classes of the target HTO from <c>[HypermediaObject(Classes)]</c>.</param>
/// <param name="Title">Title from <c>[Title]</c> attribute or XML doc summary.</param>
/// <param name="Description">Description from <c>[Description]</c> attribute or XML doc remarks.</param>
/// <param name="IsDeprecated">Whether the link is marked as deprecated.</param>
/// <param name="DeprecationMessage">Deprecation message, if any.</param>
/// <param name="IsMandatory">Whether the link property is non-nullable (mandatory).</param>
/// <param name="AccessGroups">Access groups from <c>[HypermediaAccessGroup]</c>. Empty if none.</param>
internal readonly record struct LinkMetadata(
    string PropertyName,
    EquatableArray<string> Relations,
    string TargetSchemaName,
    EquatableArray<string> TargetClasses,
    string? Title,
    string? Description,
    bool IsDeprecated,
    string? DeprecationMessage,
    bool IsMandatory,
    EquatableArray<string> AccessGroups);
