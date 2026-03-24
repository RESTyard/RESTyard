namespace RESTyard.AspNetCore.WebApi.ExtensionMethods;

/// <summary>
/// Options for the hypermedia schema endpoint.
/// </summary>
public class HypermediaSchemaEndpointOptions
{
    /// <summary>
    /// The route path for the schema endpoint.
    /// Default: <c>/hypermedia-schema</c>.
    /// </summary>
    public string Route { get; set; } = "/hypermedia-schema";
}
