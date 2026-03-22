namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// Compile-time metadata for a single data property on an HTO,
/// storing the serialized name, CLR type, forwarded attributes,
/// and XML doc comments for runtime schema generation and POCO emission.
/// </summary>
internal readonly record struct PropertyMetadata(
    /// <summary>
    /// The property name as it appears in the Siren output
    /// (respects <c>[HypermediaProperty(Name)]</c> override).
    /// </summary>
    string Name,
    /// <summary>
    /// The fully qualified CLR type name of the property,
    /// used to emit <c>typeof(T)</c> in generated code for runtime schema generation.
    /// </summary>
    string TypeFullName,
    /// <summary>
    /// Non-RESTyard attributes from the HTO property, serialized as source code strings
    /// ready to emit on the generated POCO property (e.g., <c>[JsonConverter(typeof(MyConverter))]</c>).
    /// </summary>
    EquatableArray<string> ForwardedAttributes,
    /// <summary>
    /// XML doc comment from the HTO property, to be copied verbatim onto the generated POCO property.
    /// Null when no XML doc comment is present.
    /// </summary>
    string? XmlDocComment);
