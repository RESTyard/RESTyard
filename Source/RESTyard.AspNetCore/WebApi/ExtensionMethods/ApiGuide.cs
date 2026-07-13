using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Links;
using RESTyard.MediaTypes;

namespace RESTyard.AspNetCore.WebApi.ExtensionMethods;

/// <summary>
/// Helper methods for creating API guide links.
/// </summary>
public static class ApiGuide
{
    /// <summary>
    /// Creates an <see cref="ExternalLink"/> pointing to the guide endpoint
    /// with the correct media type (<c>text/vnd.restyard.api-guide+markdown</c>).
    /// Use with <c>[Relations([DefaultHypermediaRelations.ApiGuide])]</c> on an HTO — typically
    /// the entry point — to provide a discoverable link to the authored API guide.
    /// </summary>
    /// <example>
    /// <code>
    /// [Relations([DefaultHypermediaRelations.ApiGuide])]
    /// public ExternalLink Guide { get; init; } = ApiGuide.Link();
    /// </code>
    /// </example>
    public static ExternalLink Link()
    {
        return Hypermedia.Link.External(
            new HypermediaObjectReference(
                new InternalReference(ApiGuideEndpointExtensions.RouteName)
                    .WithAvailableMediaType(DefaultMediaTypes.ApiGuide)));
    }
}
