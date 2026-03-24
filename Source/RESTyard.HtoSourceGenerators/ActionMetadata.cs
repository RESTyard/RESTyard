namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// Compile-time metadata for a single action property on an HTO.
/// </summary>
internal readonly record struct ActionMetadata(
    /// <summary>Action name from <c>[HypermediaAction(Name)]</c>, falling back to C# property name.</summary>
    string Name,
    /// <summary>Action title from <c>[HypermediaAction(Title)]</c>, <c>[Title]</c> attribute, or XML doc <c>&lt;summary&gt;</c>.</summary>
    string? Title,
    /// <summary>Description from <c>[Description]</c> attribute or XML doc <c>&lt;remarks&gt;</c>.</summary>
    string? Description,
    /// <summary>Fully qualified CLR type name of the parameter type, or null if parameterless.</summary>
    string? ParameterTypeFullName,
    /// <summary>Whether the action accepts file uploads.</summary>
    bool IsFileUpload,
    bool IsDeprecated,
    string? DeprecationMessage,
    /// <summary>Whether the action property is non-nullable (mandatory).</summary>
    bool IsMandatory,
    /// <summary>Schema name of the result entity type (from <c>ResultType</c> on the controller endpoint). Null if no result.</summary>
    string? ResultSchemaName,
    /// <summary>Siren classes of the result entity type. Null if no result.</summary>
    EquatableArray<string>? ResultClasses);
