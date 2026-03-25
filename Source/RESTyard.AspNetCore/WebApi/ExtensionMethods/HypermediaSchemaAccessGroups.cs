using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Links;
using RESTyard.Schema;

namespace RESTyard.AspNetCore.WebApi.ExtensionMethods;

/// <summary>
/// Helper methods for creating access groups endpoint links.
/// </summary>
public static class HypermediaSchemaAccessGroups
{
    /// <summary>
    /// Creates an <see cref="ExternalLink"/> pointing to the access groups discovery endpoint
    /// with the correct media type (<c>application/vnd.restyard.hypermedia-schema-access-groups+json</c>).
    /// Use with <c>[Relations(["access-groups"])]</c> on an HTO to provide a discoverable
    /// link to the available access groups.
    /// </summary>
    public static ExternalLink Link()
    {
        return Hypermedia.Link.External(
            new HypermediaObjectReference(
                new InternalReference(HypermediaSchemaEndpointExtensions.AccessGroupsRouteName)
                    .WithAvailableMediaType(SchemaMediaTypes.HypermediaSchemaAccessGroups)));
    }
}
