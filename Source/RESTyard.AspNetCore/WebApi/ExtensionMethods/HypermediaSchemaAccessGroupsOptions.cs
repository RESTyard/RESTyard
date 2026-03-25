namespace RESTyard.AspNetCore.WebApi.ExtensionMethods;

/// <summary>
/// Options for the access groups discovery endpoint.
/// </summary>
public class HypermediaSchemaAccessGroupsOptions
{
    /// <summary>
    /// Route for the access groups endpoint. Default: <c>/schema/access-groups</c>.
    /// </summary>
    public string Route { get; set; } = "/schema/access-groups";
}
