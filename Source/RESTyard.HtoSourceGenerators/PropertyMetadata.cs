using System;

namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// Compile-time metadata for a single data property on an HTO,
/// storing the serialized name, CLR type, forwarded attributes,
/// and XML doc comments for runtime schema generation and POCO emission.
/// </summary>
internal readonly struct PropertyMetadata : IEquatable<PropertyMetadata>
{
    /// <summary>
    /// The property name as it appears in the Siren output
    /// (respects <c>[HypermediaProperty(Name)]</c> override).
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The fully qualified CLR type name of the property,
    /// used to emit <c>typeof(T)</c> in generated code for runtime schema generation.
    /// </summary>
    public string TypeFullName { get; }

    /// <summary>
    /// Non-RESTyard attributes from the HTO property, serialized as source code strings
    /// ready to emit on the generated POCO property (e.g., <c>[JsonConverter(typeof(MyConverter))]</c>).
    /// </summary>
    public EquatableArray<string> ForwardedAttributes { get; }

    /// <summary>
    /// XML doc comment from the HTO property, to be copied verbatim onto the generated POCO property.
    /// Null when no XML doc comment is present.
    /// </summary>
    public string? XmlDocComment { get; }

    public PropertyMetadata(
        string name,
        string typeFullName,
        EquatableArray<string> forwardedAttributes,
        string? xmlDocComment)
    {
        Name = name;
        TypeFullName = typeFullName;
        ForwardedAttributes = forwardedAttributes;
        XmlDocComment = xmlDocComment;
    }

    public bool Equals(PropertyMetadata other)
        => Name == other.Name
           && TypeFullName == other.TypeFullName
           && ForwardedAttributes.Equals(other.ForwardedAttributes)
           && XmlDocComment == other.XmlDocComment;

    public override bool Equals(object? obj)
        => obj is PropertyMetadata other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = Name.GetHashCode() * 31;
            hash = (hash + TypeFullName.GetHashCode()) * 31;
            hash = (hash + ForwardedAttributes.GetHashCode()) * 31;
            hash += XmlDocComment?.GetHashCode() ?? 0;
            return hash;
        }
    }
}
