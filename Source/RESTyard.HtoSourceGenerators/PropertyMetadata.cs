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
