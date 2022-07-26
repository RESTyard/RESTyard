using System;
using System.Reflection;
using RESTyard.WebApi.Extensions.Hypermedia;
using RESTyard.WebApi.Extensions.Hypermedia.Actions;
using RESTyard.WebApi.Extensions.Query;
using RESTyard.WebApi.Extensions.WebApi.AttributedRoutes;
using RESTyard.WebApi.Extensions.WebApi.RouteResolver;

namespace RESTyard.WebApi.Extensions.WebApi.ExtensionMethods
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
        /// Matching type for generated routes of parameter types when using <see cref="AutoDeliverJsonSchemaForActionParameterTypes"/>
        /// </summary>
        public bool CaseSensitiveParameterMatching { get; set; }

        /// <summary>
        /// Implicitly add custom binder for parameters of hypermedia actions that derive from <see cref="IHypermediaActionParameter"/>. 
        /// Enables usage of <see cref="KeyFromUriAttribute"/> for properties of those parameter types.
        /// If set custom binder will be used for all parameter types that are not attributed differently. 
        /// If set to false custom binder will be used for parameter types explicitly attributed with <see cref="HypermediaActionParameterFromBodyAttribute"/> only.
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
        
        /// <summary>
        /// Assemblies to crawl for controller routes and hypermedia objects, if none provided the entry assembly is crawled for Hypermedia route attributes
        /// </summary>
        public Assembly[] ControllerAndHypermediaAssemblies { get; set; } = Array.Empty<Assembly>();

        /// <summary>
        /// If another route register should be used provide a type which derives from <see cref="IRouteRegister"/>
        /// </summary>
        public Type AlternateRouteRegister = null;
        
        /// <summary>
        /// If another query string builder should be used provide a type which derives from <see cref="IQueryStringBuilder"/>
        /// </summary>
        public Type AlternateQueryStringBuilder = null;
    }

    public class HypermediaConverterConfiguration
    {
        /// <summary>
        /// If true null will be written to generated hypermedia document
        /// </summary>
        public bool WriteNullProperties { get; set; } = true;
    }
}