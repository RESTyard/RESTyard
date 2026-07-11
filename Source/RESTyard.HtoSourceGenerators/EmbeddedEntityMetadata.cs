namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// Compile-time metadata for a single embedded entity property on an HTO,
/// storing the relation names, target entity info, collection flag, and nullability.
/// </summary>
/// <param name="PropertyName">The C# property name on the parent HTO (e.g. "Customers").</param>
/// <param name="Relations">Relation types from <c>[Relations]</c> attribute.</param>
/// <param name="TargetSchemaName">Schema name of the target HTO (derived via <c>DeriveSchemaName</c>).</param>
/// <param name="TargetFullyQualifiedName">Fully qualified type name of the target HTO (e.g. "MyApp.HypermediaCustomerHto").</param>
/// <param name="TargetClasses">Siren classes of the target HTO from <c>[HypermediaObject(Classes)]</c>.</param>
/// <param name="IsCollection">Whether this embedded entity represents a collection.</param>
/// <param name="Title">Title from <c>[Title]</c> attribute or XML doc summary.</param>
/// <param name="Description">Description from <c>[Description]</c> attribute or XML doc remarks.</param>
/// <param name="IsDeprecated">Whether the embedded entity is marked as deprecated.</param>
/// <param name="DeprecationMessage">Deprecation message, if any.</param>
/// <param name="IsMandatory">Whether the embedded entity property is non-nullable (mandatory).</param>
/// <param name="AccessGroups">Access groups from <c>[HypermediaAccessGroup]</c>. Empty if none.</param>
/// <param name="Location">Source location of the embedded entity property, used for diagnostics (RY0041).</param>
internal readonly record struct EmbeddedEntityMetadata(
    string PropertyName,
    EquatableArray<string> Relations,
    string TargetSchemaName,
    string TargetFullyQualifiedName,
    EquatableArray<string> TargetClasses,
    bool IsCollection,
    string? Title,
    string? Description,
    bool IsDeprecated,
    string? DeprecationMessage,
    bool IsMandatory,
    EquatableArray<string> AccessGroups,
    LocationInfo? Location);
