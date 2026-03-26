using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Links;
using RESTyard.Schema;

namespace RESTyard.AspNetCore.WebApi.ExtensionMethods;

/// <summary>
/// Helper methods for creating hypermedia schema links.
/// </summary>
public static class HypermediaSchema
{
    /// <summary>
    /// Creates an <see cref="ExternalLink"/> pointing to the schema endpoint
    /// with the correct media type (<c>application/vnd.restyard.hypermedia-schema+json</c>).
    /// Use with <c>[Relations(["schema"])]</c> on an HTO to provide a discoverable
    /// link to the API schema from the entry point.
    /// </summary>
    /// <example>
    /// <code>
    /// [Relations(["schema"])]
    /// public ExternalLink Schema { get; init; } = HypermediaSchema.Link();
    /// </code>
    /// </example>
    public static ExternalLink Link()
    {
        return Hypermedia.Link.External(
            new HypermediaObjectReference(
                new InternalReference(HypermediaSchemaEndpointExtensions.RouteName)
                    .WithAvailableMediaType(SchemaMediaTypes.HypermediaApiSchema)));
    }

    /// <summary>
    /// Creates an <see cref="ExternalLink"/> pointing to the schema endpoint with access group filter
    /// query parameters.
    /// </summary>
    /// <param name="filter">Filter parameters. Set either <c>AccessGroups</c> or <c>ExcludeAccessGroups</c>.</param>
    public static ExternalLink Link(HypermediaSchemaFilterParameters filter)
    {
        return Hypermedia.Link.External(
            new HypermediaObjectReference(
                new InternalReference(HypermediaSchemaEndpointExtensions.RouteName, filter)
                    .WithAvailableMediaType(SchemaMediaTypes.HypermediaApiSchema)));
    }
}
