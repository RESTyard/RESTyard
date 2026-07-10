namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// Compile-time metadata for a single action property on an HTO.
/// </summary>
/// <param name="PropertyName">The C# property name on the HTO class, used to emit <c>hto.PropertyName</c>.</param>
/// <param name="DeclaringClassName">Name of the class that declares the action property — the base HTO
/// for inherited actions. Used to resolve controller <c>ResultType</c> mappings keyed by the base HTO.</param>
/// <param name="Name">Action name from <c>[HypermediaAction(Name)]</c>, falling back to C# property name.</param>
/// <param name="Title">Action title from <c>[HypermediaAction(Title)]</c>, <c>[Title]</c> attribute, or XML doc summary.</param>
/// <param name="Description">Description from <c>[Description]</c> attribute or XML doc remarks.</param>
/// <param name="ParameterTypeFullName">Fully qualified CLR type name of the parameter type, or null if parameterless.</param>
/// <param name="IsFileUpload">Whether the action accepts file uploads.</param>
/// <param name="IsDeprecated">Whether the action is marked as deprecated.</param>
/// <param name="DeprecationMessage">Deprecation message, if any.</param>
/// <param name="IsMandatory">Whether the action property is non-nullable (mandatory).</param>
/// <param name="ResultSchemaName">Schema name of the result entity type (from ResultType on the controller endpoint). Null if no result.</param>
/// <param name="ResultClasses">Siren classes of the result entity type. Null if no result.</param>
/// <param name="UserClasses">User-defined classes from <c>[HypermediaAction(Classes = [...])]</c>. Empty if none.</param>
/// <param name="AccessGroups">Access groups from <c>[HypermediaAccessGroup]</c>. Empty if none.</param>
internal readonly record struct ActionMetadata(
    string PropertyName,
    string DeclaringClassName,
    string Name,
    string? Title,
    string? Description,
    string? ParameterTypeFullName,
    bool IsFileUpload,
    bool IsDeprecated,
    string? DeprecationMessage,
    bool IsMandatory,
    string? ResultSchemaName,
    EquatableArray<string>? ResultClasses,
    EquatableArray<string> UserClasses,
    EquatableArray<string> AccessGroups);
