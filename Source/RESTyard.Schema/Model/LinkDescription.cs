using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RESTyard.Schema.Model;

/// <summary>
/// Describes a link on an entity type.
/// </summary>
public class LinkDescription
{
    /// <summary>
    /// Link relation types (e.g. "self", "item").
    /// </summary>
    [JsonPropertyName("relations")]
    public IReadOnlyList<string> Relations { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Name of the target entity type.
    /// </summary>
    [JsonPropertyName("targetName")]
    public string TargetName { get; set; } = string.Empty;

    /// <summary>
    /// Siren classes of the target entity type.
    /// </summary>
    [JsonPropertyName("targetClasses")]
    public IReadOnlyList<string> TargetClasses { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Media type of the linked resource.
    /// </summary>
    [JsonPropertyName("mediaType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? MediaType { get; set; }

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
    /// Access groups required for this link. Null means no restriction (public).
    /// </summary>
    [JsonPropertyName("accessGroups")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? AccessGroups { get; set; }

    /// <summary>
    /// Whether this link is always present on the entity.
    /// </summary>
    [JsonPropertyName("isMandatory")]
    public bool IsMandatory { get; set; }

    /// <summary>
    /// Whether this link is deprecated.
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
