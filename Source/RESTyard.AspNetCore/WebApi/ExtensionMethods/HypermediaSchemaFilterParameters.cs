namespace RESTyard.AspNetCore.WebApi.ExtensionMethods;

/// <summary>
/// Parameters for filtering the hypermedia schema by access groups.
/// Used with <see cref="HypermediaSchema.Link(HypermediaSchemaFilterParameters)"/>
/// to create links to filtered schema endpoints.
/// </summary>
public class HypermediaSchemaFilterParameters
{
    /// <summary>
    /// Include mode: comma-separated access groups. Elements visible to any of these groups are kept.
    /// </summary>
    public string? AccessGroups { get; init; }

    /// <summary>
    /// Exclude mode: comma-separated access groups. Elements matching any of these groups are removed.
    /// </summary>
    public string? ExcludeAccessGroups { get; init; }
}
