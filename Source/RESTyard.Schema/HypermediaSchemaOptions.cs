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
    /// Default: null (omitted from schema). Set explicitly — not auto-detected.
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

    /// <summary>
    /// Controls how dangling cross-references (link/embedded <c>targetName</c>, action
    /// <c>resultName</c> not matching any entity type) are handled during schema composition.
    /// Dangling references indicate a missing or unregistered HTO and are always logged as
    /// warnings. When <c>true</c>, each unresolved name additionally produces a placeholder
    /// entity type (no properties, links, or actions) so the schema endpoint and diagrams
    /// remain functional while the API is still being built.
    /// Default: <c>false</c> (warn only, no placeholders).
    /// </summary>
    public bool AllowUnresolvedReferences { get; set; }
}
