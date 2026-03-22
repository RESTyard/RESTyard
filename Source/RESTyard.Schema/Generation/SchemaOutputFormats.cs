using System;

namespace RESTyard.Schema.Generation;

/// <summary>
/// Specifies which output artifacts to generate.
/// </summary>
[Flags]
public enum SchemaOutputFormats
{
    /// <summary>Schema as JSON file (<c>hypermedia-api-schema.json</c>).</summary>
    JsonHypermediaApiSchema = 1,

    /// <summary>Mermaid API Map (<c>api-map.md</c>).</summary>
    MermaidApiMap = 2,

    /// <summary>Mermaid class diagram of HTO types (<c>htos.md</c>).</summary>
    MermaidHtos = 4,

    /// <summary>Markdown API documentation (<c>api-documentation.md</c>).</summary>
    MarkdownApiDocumentation = 8,

    /// <summary>All output artifacts.</summary>
    All = JsonHypermediaApiSchema | MermaidApiMap | MermaidHtos | MarkdownApiDocumentation,
}
