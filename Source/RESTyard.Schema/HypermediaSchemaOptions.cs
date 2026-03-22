namespace RESTyard.Schema;

/// <summary>
/// Configuration options for the RESTyard hypermedia schema feature.
/// Passed to <c>AddHypermediaSchema()</c> to configure schema metadata.
/// All properties are nullable — when null, sensible defaults are applied.
/// </summary>
public class HypermediaSchemaOptions
{
    /// <summary>
    /// Human-readable title of the API.
    /// Default: entry assembly name.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Human-readable description of the API.
    /// Default: null (omitted from schema).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Version of the API described by this schema.
    /// Default: entry assembly informational version or assembly version.
    /// </summary>
    public string? ApiVersion { get; set; }

    /// <summary>
    /// Name of the entry point entity type (references <c>EntityTypeSchema.Name</c>).
    /// Default: auto-detected from entity types with Siren class <c>"EntryPoint"</c>.
    /// </summary>
    public string? EntryPointName { get; set; }

    /// <summary>
    /// URL to external documentation for the API.
    /// Default: null (omitted from schema).
    /// </summary>
    public string? ExternalDocsUrl { get; set; }
}
