namespace WebApi.HypermediaExtensions.WebApi.ExtensionMethods
{
    using global::WebApi.HypermediaExtensions.Hypermedia;
    using global::WebApi.HypermediaExtensions.Hypermedia.Actions;
    using global::WebApi.HypermediaExtensions.WebApi.AttributedRoutes;
    using global::WebApi.HypermediaExtensions.WebApi.Formatter;
    using global::WebApi.HypermediaExtensions.WebApi.RouteResolver;

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
        /// Matching type for generated routes of parameter types when using <see cref="AutoDeliverJsonSchemaForActionParameterTypes"/>
        /// </summary>
        public bool CaseSensitiveParameterMatching { get; set; }

        /// <summary>
        /// Implicitly add custom binder for parameters of hypermedia actions that derive from <see cref="IHypermediaActionParameter"/>. 
        /// Enables usage of <see cref="KeyFromUriAttribute"/> for properties of those parameter types.
        /// </summary>
        public bool ImplicitHypermediaActionParameterBinders { get; set; } = true;

        /// <summary>
        /// Configuration for Url generation
        /// </summary>
        public HypermediaUrlConfig HypermediaUrlConfig { get; set; } = new HypermediaUrlConfig();


        /// <summary>
        /// Configuration for hypermedia document generation
        /// </summary>
        public HypermediaConverterConfiguration HypermediaConverterConfiguration { get; set; } = new HypermediaConverterConfiguration();
    }

    public class HypermediaConverterConfiguration
    {
        /// <summary>
        /// If true null will be written to generated hypermedia document
        /// </summary>
        public bool WriteNullProperties { get; set; } = true;
    }
}