using System.Collections.Generic;
using System.Text.Json.Serialization;
using Json.Schema;

namespace RESTyard.Schema.Model;

/// <summary>
/// Describes an action available on an entity type.
/// </summary>
public class ActionDescription
{
    /// <summary>
    /// Name of the action.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

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
    /// Content type of the action request body.
    /// </summary>
    [JsonPropertyName("contentType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContentType { get; set; }

    /// <summary>
    /// Contains a JSON Schema describing the action's parameters.
    /// </summary>
    [JsonPropertyName("parameterSchema")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonSchema? ParameterSchema { get; set; }

    /// <summary>
    /// Name of the result entity type, if the action returns one.
    /// </summary>
    [JsonPropertyName("resultName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ResultName { get; set; }

    /// <summary>
    /// Siren classes of the result entity type.
    /// </summary>
    [JsonPropertyName("resultClasses")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? ResultClasses { get; set; }

    /// <summary>
    /// Whether this action is always present on the entity.
    /// </summary>
    [JsonPropertyName("isMandatory")]
    public bool IsMandatory { get; set; }

    /// <summary>
    /// Whether this action accepts a file upload.
    /// </summary>
    [JsonPropertyName("isFileUpload")]
    public bool IsFileUpload { get; set; }

    /// <summary>
    /// Whether this action is deprecated.
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
