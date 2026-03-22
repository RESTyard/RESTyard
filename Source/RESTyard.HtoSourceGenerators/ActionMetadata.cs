using System;

namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// Compile-time metadata for a single action property on an HTO.
/// </summary>
internal readonly struct ActionMetadata : IEquatable<ActionMetadata>
{
    /// <summary>
    /// Action name from <c>[HypermediaAction(Name)]</c>, falling back to C# property name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Action title from <c>[HypermediaAction(Title)]</c>, <c>[Title]</c> attribute, or XML doc <c>&lt;summary&gt;</c>.
    /// </summary>
    public string? Title { get; }

    /// <summary>
    /// Description from <c>[Description]</c> attribute or XML doc <c>&lt;remarks&gt;</c>.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Fully qualified CLR type name of the parameter type, or null if parameterless.
    /// Used to emit <c>typeof(T)</c> for runtime schema generation via <c>IJsonSchemaFactory</c>.
    /// </summary>
    public string? ParameterTypeFullName { get; }

    /// <summary>
    /// Whether the action accepts file uploads.
    /// </summary>
    public bool IsFileUpload { get; }

    /// <summary>
    /// Whether the action property is non-nullable (mandatory).
    /// </summary>
    public bool IsDeprecated { get; }
    public string? DeprecationMessage { get; }
    public bool IsMandatory { get; }

    public ActionMetadata(
        string name,
        string? title,
        string? description,
        string? parameterTypeFullName,
        bool isFileUpload,
        bool isDeprecated,
        string? deprecationMessage,
        bool isMandatory)
    {
        Name = name;
        Title = title;
        Description = description;
        ParameterTypeFullName = parameterTypeFullName;
        IsFileUpload = isFileUpload;
        IsDeprecated = isDeprecated;
        DeprecationMessage = deprecationMessage;
        IsMandatory = isMandatory;
    }

    public bool Equals(ActionMetadata other)
        => Name == other.Name
           && Title == other.Title
           && Description == other.Description
           && ParameterTypeFullName == other.ParameterTypeFullName
           && IsFileUpload == other.IsFileUpload
           && IsDeprecated == other.IsDeprecated
           && DeprecationMessage == other.DeprecationMessage
           && IsMandatory == other.IsMandatory;

    public override bool Equals(object? obj)
        => obj is ActionMetadata other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 31 + Name.GetHashCode();
            hash = hash * 31 + (Title?.GetHashCode() ?? 0);
            hash = hash * 31 + (Description?.GetHashCode() ?? 0);
            hash = hash * 31 + IsDeprecated.GetHashCode();
            hash = hash * 31 + (DeprecationMessage?.GetHashCode() ?? 0);
            hash = hash * 31 + (ParameterTypeFullName?.GetHashCode() ?? 0);
            hash = hash * 31 + IsFileUpload.GetHashCode();
            hash = hash * 31 + IsMandatory.GetHashCode();
            return hash;
        }
    }
}
