namespace RESTyard.Schema.Markdown;

/// <summary>
/// Options controlling the output of <see cref="MarkdownMapper.ToDocumentation"/>.
/// </summary>
public class MarkdownMapperOptions
{
    /// <summary>
    /// When true (default), includes a Table of Contents at the top.
    /// </summary>
    public bool IncludeTableOfContents { get; set; } = true;

    /// <summary>
    /// When true (default), embeds a Mermaid entity graph diagram after the header.
    /// </summary>
    public bool IncludeDiagram { get; set; } = true;
}
