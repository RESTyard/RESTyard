using System;

namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// Compile-time metadata extracted from an HTO class, used by the incremental generator
/// to produce schema and mapper source code.
/// </summary>
internal readonly struct HtoMetadata : IEquatable<HtoMetadata>
{
    public string Namespace { get; }
    public string ClassName { get; }
    public string SchemaName { get; }
    public string? Title { get; }
    public EquatableArray<string> Classes { get; }

    public HtoMetadata(
        string ns,
        string className,
        string schemaName,
        string? title,
        EquatableArray<string> classes)
    {
        Namespace = ns;
        ClassName = className;
        SchemaName = schemaName;
        Title = title;
        Classes = classes;
    }

    public bool Equals(HtoMetadata other)
        => Namespace == other.Namespace
           && ClassName == other.ClassName
           && SchemaName == other.SchemaName
           && Title == other.Title
           && Classes.Equals(other.Classes);

    public override bool Equals(object? obj)
        => obj is HtoMetadata other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 31 + Namespace.GetHashCode();
            hash = hash * 31 + ClassName.GetHashCode();
            hash = hash * 31 + SchemaName.GetHashCode();
            hash = hash * 31 + (Title?.GetHashCode() ?? 0);
            hash = hash * 31 + Classes.GetHashCode();
            return hash;
        }
    }
}
