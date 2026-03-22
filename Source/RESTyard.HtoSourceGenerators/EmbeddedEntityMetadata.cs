using System;

namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// Compile-time metadata for a single embedded entity property on an HTO,
/// storing the relation names, target entity info, collection flag, and nullability.
/// </summary>
internal readonly struct EmbeddedEntityMetadata : IEquatable<EmbeddedEntityMetadata>
{
    /// <summary>
    /// Relation types from <c>[Relations]</c> attribute.
    /// </summary>
    public EquatableArray<string> Relations { get; }

    /// <summary>
    /// Schema name of the target HTO (derived via <c>DeriveSchemaName</c>).
    /// </summary>
    public string TargetSchemaName { get; }

    /// <summary>
    /// Siren classes of the target HTO from <c>[HypermediaObject(Classes)]</c>.
    /// </summary>
    public EquatableArray<string> TargetClasses { get; }

    /// <summary>
    /// Whether this embedded entity represents a collection.
    /// </summary>
    public bool IsCollection { get; }

    /// <summary>
    /// Title from <c>[Title]</c> attribute or XML doc <c>&lt;summary&gt;</c>.
    /// </summary>
    public string? Title { get; }

    /// <summary>
    /// Description from <c>[Description]</c> attribute or XML doc <c>&lt;summary&gt;</c> (fallback).
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Whether the embedded entity property is non-nullable (mandatory).
    /// </summary>
    public bool IsMandatory { get; }

    public EmbeddedEntityMetadata(
        EquatableArray<string> relations,
        string targetSchemaName,
        EquatableArray<string> targetClasses,
        bool isCollection,
        string? title,
        string? description,
        bool isMandatory)
    {
        Relations = relations;
        TargetSchemaName = targetSchemaName;
        TargetClasses = targetClasses;
        IsCollection = isCollection;
        Title = title;
        Description = description;
        IsMandatory = isMandatory;
    }

    public bool Equals(EmbeddedEntityMetadata other)
        => Relations.Equals(other.Relations)
           && TargetSchemaName == other.TargetSchemaName
           && TargetClasses.Equals(other.TargetClasses)
           && IsCollection == other.IsCollection
           && Title == other.Title
           && Description == other.Description
           && IsMandatory == other.IsMandatory;

    public override bool Equals(object? obj)
        => obj is EmbeddedEntityMetadata other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 31 + Relations.GetHashCode();
            hash = hash * 31 + TargetSchemaName.GetHashCode();
            hash = hash * 31 + TargetClasses.GetHashCode();
            hash = hash * 31 + IsCollection.GetHashCode();
            hash = hash * 31 + (Title?.GetHashCode() ?? 0);
            hash = hash * 31 + (Description?.GetHashCode() ?? 0);
            hash = hash * 31 + IsMandatory.GetHashCode();
            return hash;
        }
    }
}
