namespace RESTyard.Schema.Generation;

/// <summary>
/// Options controlling the output of schema generation, including mapper-specific settings.
/// </summary>
public class SchemaGeneratorOptions
{
    /// <summary>
    /// Include property lines in Mermaid HTO diagram boxes. Default: true.
    /// CLI: <c>--mermaid-include-properties false</c>
    /// Applies to the <c>mermaid-htos</c> artifact.
    /// </summary>
    public bool MermaidIncludeProperties { get; set; } = true;

    /// <summary>
    /// Include action lines in Mermaid HTO diagram boxes. Default: true.
    /// CLI: <c>--mermaid-include-actions false</c>
    /// Applies to the <c>mermaid-htos</c> artifact.
    /// </summary>
    public bool MermaidIncludeActions { get; set; } = true;

    /// <summary>
    /// Include a Table of Contents in Markdown API documentation. Default: true.
    /// CLI: <c>--markdown-include-toc false</c>
    /// Applies to the <c>markdown-api-documentation</c> artifact.
    /// </summary>
    public bool MarkdownIncludeToc { get; set; } = true;

    /// <summary>
    /// Include a Mermaid diagram in Markdown API documentation. Default: true.
    /// CLI: <c>--markdown-include-diagram false</c>
    /// Applies to the <c>markdown-api-documentation</c> artifact.
    /// </summary>
    public bool MarkdownIncludeDiagram { get; set; } = true;

    /// <summary>
    /// Wrap Mermaid output in a Markdown document with a title and fenced code block,
    /// making the <c>.md</c> files directly renderable in GitHub, GitLab, etc.
    /// Default: true. When false, raw Mermaid syntax is emitted.
    /// CLI: <c>--mermaid-wrap-markdown false</c>
    /// Applies to <c>mermaid-api-map</c> and <c>mermaid-htos</c> artifacts.
    /// </summary>
    public bool MermaidWrapMarkdown { get; set; } = true;
}
