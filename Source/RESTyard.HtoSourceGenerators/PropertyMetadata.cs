using System;

namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// Compile-time metadata for a single data property on an HTO,
/// storing the serialized name and the CLR type for runtime schema generation.
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

    public PropertyMetadata(string name, string typeFullName)
    {
        Name = name;
        TypeFullName = typeFullName;
    }

    public bool Equals(PropertyMetadata other)
        => Name == other.Name && TypeFullName == other.TypeFullName;

    public override bool Equals(object? obj)
        => obj is PropertyMetadata other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            return (Name.GetHashCode() * 31) + TypeFullName.GetHashCode();
        }
    }
}
