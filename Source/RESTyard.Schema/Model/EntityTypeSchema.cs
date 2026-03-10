using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RESTyard.Schema.Model;

/// <summary>
/// Describes a single entity type in the hypermedia API.
/// </summary>
public class EntityTypeSchema
{
    /// <summary>
    /// Unique name of this entity type.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Siren classes for this entity type.
    /// </summary>
    [JsonPropertyName("classes")]
    public IReadOnlyList<string> Classes { get; set; }

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
    /// Contains a JSON Schema describing the entity's properties.
    /// </summary>
    [JsonPropertyName("propertiesSchema")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? PropertiesSchema { get; set; }

    /// <summary>
    /// Links available on this entity type.
    /// </summary>
    [JsonPropertyName("links")]
    public IReadOnlyList<LinkDescription> Links { get; set; }

    /// <summary>
    /// Actions available on this entity type.
    /// </summary>
    [JsonPropertyName("actions")]
    public IReadOnlyList<ActionDescription> Actions { get; set; }

    /// <summary>
    /// Embedded entities within this entity type.
    /// </summary>
    [JsonPropertyName("embeddedEntities")]
    public IReadOnlyList<EmbeddedEntityDescription> EmbeddedEntities { get; set; }

    /// <summary>
    /// Whether this entity type is deprecated.
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
