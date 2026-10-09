using System;
using System.Reflection;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.AspNetCore.JsonSchema;
using RESTyard.AspNetCore.Query;
using RESTyard.AspNetCore.WebApi.AttributedRoutes;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace RESTyard.AspNetCore.WebApi.ExtensionMethods
{
    /// <summary>
    /// General options for the extensions
    /// </summary>
    public class HypermediaExtensionsOptions
    {
        /// <summary>
        /// Enable to return a default route if a linked <see cref="IHypermediaObject"/> has no corresponding route.
        /// </summary>
        public bool ReturnDefaultRouteForUnknownHto { get; set; }

        /// <summary>
        /// Route segment which will be appended to "scheme://authority/" of the request.
        /// Default: "unknown/object/route"
        /// </summary>
        public string DefaultRouteSegmentForUnknownHto { get; set; } = "unknown/object/route";

        /// <summary>
        /// Automatically deliver json schema for hypermedia action parameters. Custom schemas can still be delivered by implementing controller methods attributed
        /// with <see cref="HypermediaActionParameterInfoEndpointAttribute{TParameter}"/> attribute.
        /// </summary>
        public bool AutoDeliverJsonSchemaForActionParameterTypes { get; set; } = true;

        /// <summary>
        /// Matching type for generated routes of parameter types when using <see cref="AutoDeliverJsonSchemaForActionParameterTypes"/>
        /// </summary>
        public bool CaseSensitiveParameterMatching { get; set; }

        /// <summary>
        /// Implicitly use the hypermedia form binder for file-upload action parameters
        /// (<see cref="HypermediaFileUploadActionParameter"/>) that are bound from a form.
        /// If set to false, the form binder is used only for parameters attributed with
        /// <see cref="HypermediaUploadParameterFromFormAttribute"/>.
        /// Action parameter bodies always bind through the standard <c>[FromBody]</c> path.
        /// </summary>
        public bool ImplicitHypermediaActionParameterBinders { get; set; } = true;

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
        public Type? AlternateRouteRegister = null;
        
        /// <summary>
        /// If another query string builder should be used provide a type which derives from <see cref="IQueryStringBuilder"/>
        /// </summary>
        public Type? AlternateQueryStringBuilder = null;
        
        /// <summary>
        /// If another JSON schema factory should be used provide a type which derives from <see cref="IJsonSchemaFactory"/>
        /// </summary>
        public Type? AlternateJsonSchemaFactory = null;
    }

    public class HypermediaConverterConfiguration
    {
        /// <summary>
        /// If true null will be written to generated hypermedia document
        /// </summary>
        public bool WriteNullProperties { get; set; } = true;
    }
}