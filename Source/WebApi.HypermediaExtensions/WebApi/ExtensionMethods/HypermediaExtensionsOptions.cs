using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.Hypermedia.Actions;
using WebApi.HypermediaExtensions.WebApi.AttributedRoutes;
using WebApi.HypermediaExtensions.WebApi.RouteResolver;

namespace WebApi.HypermediaExtensions.WebApi.ExtensionMethods
{
    /// <summary>
    /// General options for the extensions
    /// </summary>
    public class HypermediaExtensionsOptions
    {
        /// <summary>
        /// Enable to return a default route if a linked <see cref="HypermediaObject"/> has no corresponding route.
        /// </summary>
        public bool ReturnDefaultRouteForUnknownHto { get; set; }

        /// <summary>
        /// Route segment which will be appended to "scheme://authority/" of the request.
        /// Default: "unknown/object/route"
        /// </summary>
        public string DefaultRouteSegmentForUnknownHto { get; set; } = "unknown/object/route";

        /// <summary>
        /// Automatically deliver json schema for hypermedia action parameters. Custom schemas can still be delivered by implementing controller methods attributed
        /// with <see cref="HttpGetHypermediaActionParameterInfo"/> attibute.
        /// </summary>
        public bool AutoDeliverJsonSchemaForActionParameterTypes { get; set; } = true;

        /// <summary>
        /// Implictly add custom binder for parameters of hypermedia actions that derive from <see cref="IHypermediaActionParameter"/>. 
        /// Enables usage of <see cref="KeyFromUriAttribute"/> for properties of those parameter types.
        /// </summary>
        public bool ImplicitHypermediaActionParameterBinders { get; set; } = true;

        /// <summary>
        /// Tells the extensions that the routing will use lowercase URLs
        /// E.g. needed so generated routes for parameter schemas can be matched correctly
        /// </summary>
        public bool LowercaseUrls { get; set; }
    }
}