using System;

namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// Compile-time metadata for a single link property on an HTO,
/// storing the relation names, target entity info, and nullability.
/// </summary>
internal readonly struct LinkMetadata : IEquatable<LinkMetadata>
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
    /// Whether the link property is non-nullable (mandatory).
    /// </summary>
    public bool IsMandatory { get; }

    public LinkMetadata(
        EquatableArray<string> relations,
        string targetSchemaName,
        EquatableArray<string> targetClasses,
        bool isMandatory)
    {
        Relations = relations;
        TargetSchemaName = targetSchemaName;
        TargetClasses = targetClasses;
        IsMandatory = isMandatory;
    }

    public bool Equals(LinkMetadata other)
        => Relations.Equals(other.Relations)
           && TargetSchemaName == other.TargetSchemaName
           && TargetClasses.Equals(other.TargetClasses)
           && IsMandatory == other.IsMandatory;

    public override bool Equals(object? obj)
        => obj is LinkMetadata other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 31 + Relations.GetHashCode();
            hash = hash * 31 + TargetSchemaName.GetHashCode();
            hash = hash * 31 + TargetClasses.GetHashCode();
            hash = hash * 31 + IsMandatory.GetHashCode();
            return hash;
        }
    }
}
