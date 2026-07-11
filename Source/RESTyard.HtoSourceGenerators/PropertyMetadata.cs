namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// Compile-time metadata for a single data property on an HTO,
/// storing the serialized name, CLR type, forwarded attributes,
/// and XML doc comments for runtime schema generation and POCO emission.
/// </summary>
/// <param name="Name">The property name as it appears in the Siren output and on the generated POCO (respects <c>[HypermediaProperty(Name)]</c> override).</param>
/// <param name="OriginalName">The original C# property name on the HTO class. Used by <c>ToSiren()</c> to emit <c>hto.OriginalName</c>. Same as <paramref name="Name"/> when no override is present.</param>
/// <param name="TypeFullName">The fully qualified CLR type name of the property, used to emit <c>typeof(T)</c> in generated code.</param>
/// <param name="ForwardedAttributes">Non-RESTyard attributes from the HTO property, serialized as source code strings ready to emit on the generated POCO property.</param>
/// <param name="XmlDocComment">XML doc comment from the HTO property, to be copied verbatim onto the generated POCO property. Null when absent.</param>
internal readonly record struct PropertyMetadata(
    string Name,
    string OriginalName,
    string TypeFullName,
    EquatableArray<string> ForwardedAttributes,
    string? XmlDocComment);

/// <summary>
/// A <c>[HypermediaProperty(Name)]</c> override that is not a valid C# identifier —
/// the generated POCO uses the name structurally, so the override is ignored and
/// reported as an RY0022 warning.
/// </summary>
/// <param name="PropertyName">The original C# property name on the HTO class.</param>
/// <param name="InvalidName">The rejected override value.</param>
/// <param name="Location">Location of the <c>[HypermediaProperty]</c> attribute (or the property) for the diagnostic.</param>
internal readonly record struct InvalidPropertyNameOverride(
    string PropertyName,
    string InvalidName,
    LocationInfo? Location);

/// <summary>
/// A property name plus its source location — used for diagnostics that only need to
/// point at a property (e.g. missing <c>[Relations]</c>, RY0020/RY0021).
/// </summary>
internal readonly record struct PropertyRef(
    string PropertyName,
    LocationInfo? Location);
