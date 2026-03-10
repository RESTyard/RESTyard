using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RESTyard.Schema.Model;

/// <summary>
/// Describes an embedded entity within an entity type.
/// </summary>
public class EmbeddedEntityDescription
{
    /// <summary>
    /// Relation types for the embedded entity.
    /// </summary>
    [JsonPropertyName("relations")]
    public IReadOnlyList<string> Relations { get; set; }

    /// <summary>
    /// Name of the target entity type.
    /// </summary>
    [JsonPropertyName("targetName")]
    public string TargetName { get; set; }

    /// <summary>
    /// Siren classes of the target entity type.
    /// </summary>
    [JsonPropertyName("targetClasses")]
    public IReadOnlyList<string> TargetClasses { get; set; }

    /// <summary>
    /// Human-readable title.
    /// </summary>
    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Title { get; set; }

    /// <summary>
    /// Human-readable description.
    /// </summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }

    /// <summary>
    /// Whether this embedded entity represents a collection.
    /// </summary>
    [JsonPropertyName("isCollection")]
    public bool IsCollection { get; set; }

    /// <summary>
    /// Whether this embedded entity is always present.
    /// </summary>
    [JsonPropertyName("isMandatory")]
    public bool IsMandatory { get; set; }

    /// <summary>
    /// Whether this embedded entity is deprecated.
    /// </summary>
    [JsonPropertyName("isDeprecated")]
    public bool IsDeprecated { get; set; }

    /// <summary>
    /// Message explaining the deprecation reason or migration path.
    /// </summary>
    [JsonPropertyName("deprecationMessage")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DeprecationMessage { get; set; }
}
