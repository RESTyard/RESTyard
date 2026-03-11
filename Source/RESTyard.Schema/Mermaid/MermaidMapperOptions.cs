namespace RESTyard.Schema.Mermaid;

/// <summary>
/// Options controlling what details are included in the Mermaid class diagram.
/// </summary>
public class MermaidMapperOptions
{
    /// <summary>
    /// When true, property lines are included in class boxes. Default: true.
    /// Set to false to reduce diagram size for APIs with many properties.
    /// </summary>
    public bool IncludeProperties { get; set; } = true;

    /// <summary>
    /// When true, action lines are included in class boxes. Default: true.
    /// Set to false to reduce diagram size for APIs with many actions.
    /// </summary>
    public bool IncludeActions { get; set; } = true;
}
