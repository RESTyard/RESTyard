using System;
using System.Linq;

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
    public string? Description { get; }
    public bool IsDeprecated { get; }
    public string? DeprecationMessage { get; }
    public EquatableArray<string> Classes { get; }
    public EquatableArray<PropertyMetadata> Properties { get; }
    public EquatableArray<LinkMetadata> Links { get; }
    public EquatableArray<ActionMetadata> Actions { get; }
    public EquatableArray<EmbeddedEntityMetadata> EmbeddedEntities { get; }

    /// <summary>
    /// Property names that are of type <c>IEmbeddedEntity</c> but missing <c>[Relations]</c>.
    /// Used to emit RY0020 warnings during source generation.
    /// </summary>
    public EquatableArray<string> EmbeddedEntityPropertiesWithoutRelations { get; }

    /// <summary>
    /// Property names that are of type <c>ILink</c> but missing <c>[Relations]</c>.
    /// Used to emit RY0021 warnings during source generation.
    /// </summary>
    public EquatableArray<string> LinkPropertiesWithoutRelations { get; }

    public HtoMetadata(
        string ns,
        string className,
        string schemaName,
        string? title,
        string? description,
        bool isDeprecated,
        string? deprecationMessage,
        EquatableArray<string> classes,
        EquatableArray<PropertyMetadata> properties,
        EquatableArray<LinkMetadata> links,
        EquatableArray<ActionMetadata> actions,
        EquatableArray<EmbeddedEntityMetadata> embeddedEntities,
        EquatableArray<string> embeddedEntityPropertiesWithoutRelations,
        EquatableArray<string> linkPropertiesWithoutRelations)
    {
        Namespace = ns;
        ClassName = className;
        SchemaName = schemaName;
        Title = title;
        Description = description;
        IsDeprecated = isDeprecated;
        DeprecationMessage = deprecationMessage;
        Classes = classes;
        Properties = properties;
        Links = links;
        Actions = actions;
        EmbeddedEntities = embeddedEntities;
        EmbeddedEntityPropertiesWithoutRelations = embeddedEntityPropertiesWithoutRelations;
        LinkPropertiesWithoutRelations = linkPropertiesWithoutRelations;
    }

    /// <summary>
    /// Whether GetSchema() needs an <c>IJsonSchemaFactory</c> parameter
    /// (true when there are data properties or parameterized actions).
    /// </summary>
    public bool NeedsSchemaFactory
        => Properties.Length > 0 || Actions.Any(a => a.ParameterTypeFullName != null);

    public bool Equals(HtoMetadata other)
        => Namespace == other.Namespace
           && ClassName == other.ClassName
           && SchemaName == other.SchemaName
           && Title == other.Title
           && Description == other.Description
           && IsDeprecated == other.IsDeprecated
           && DeprecationMessage == other.DeprecationMessage
           && Classes.Equals(other.Classes)
           && Properties.Equals(other.Properties)
           && Links.Equals(other.Links)
           && Actions.Equals(other.Actions)
           && EmbeddedEntities.Equals(other.EmbeddedEntities)
           && EmbeddedEntityPropertiesWithoutRelations.Equals(other.EmbeddedEntityPropertiesWithoutRelations)
           && LinkPropertiesWithoutRelations.Equals(other.LinkPropertiesWithoutRelations);

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
            hash = hash * 31 + (Description?.GetHashCode() ?? 0);
            hash = hash * 31 + IsDeprecated.GetHashCode();
            hash = hash * 31 + (DeprecationMessage?.GetHashCode() ?? 0);
            hash = hash * 31 + Classes.GetHashCode();
            hash = hash * 31 + Properties.GetHashCode();
            hash = hash * 31 + Links.GetHashCode();
            hash = hash * 31 + Actions.GetHashCode();
            hash = hash * 31 + EmbeddedEntities.GetHashCode();
            hash = hash * 31 + EmbeddedEntityPropertiesWithoutRelations.GetHashCode();
            hash = hash * 31 + LinkPropertiesWithoutRelations.GetHashCode();
            return hash;
        }
    }
}
