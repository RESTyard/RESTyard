using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Json.Schema;

namespace RESTyard.Schema.Model;

/// <summary>
/// Top-level schema describing a RESTyard hypermedia API.
/// </summary>
public class HypermediaApiSchema
{
    /// <summary>
    /// Version of the schema format itself.
    /// </summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; set; } = string.Empty;

    /// <summary>
    /// Version of the described API.
    /// </summary>
    [JsonPropertyName("apiVersion")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ApiVersion { get; set; }

    /// <summary>
    /// Human-readable title of the API.
    /// </summary>
    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Title { get; set; }

    /// <summary>
    /// Human-readable description of the API.
    /// </summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }

    /// <summary>
    /// URL to external documentation for the API.
    /// </summary>
    [JsonPropertyName("externalDocsUrl")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ExternalDocsUrl { get; set; }

    /// <summary>
    /// Name of the entry point entity type.
    /// </summary>
    [JsonPropertyName("entryPointName")]
    public string EntryPointName { get; set; } = string.Empty;

    /// <summary>
    /// All entity types defined in the API.
    /// </summary>
    [JsonPropertyName("entityTypes")]
    public IReadOnlyList<EntityTypeSchema> EntityTypes { get; set; } = Array.Empty<EntityTypeSchema>();

    /// <summary>
    /// Shared type definitions. Each value contains a JSON Schema.
    /// </summary>
    [JsonPropertyName("definitions")]
    public IDictionary<string, JsonSchema> Definitions { get; set; } = new Dictionary<string, JsonSchema>();
}
