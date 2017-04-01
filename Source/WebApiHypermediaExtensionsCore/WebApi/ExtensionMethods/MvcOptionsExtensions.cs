using Microsoft.AspNetCore.Mvc;
using WebApiHypermediaExtensionsCore.Query;
using WebApiHypermediaExtensionsCore.WebApi.AttributedRoutes;
using WebApiHypermediaExtensionsCore.WebApi.Formatter;
using WebApiHypermediaExtensionsCore.WebApi.RouteResolver;

namespace WebApiHypermediaExtensionsCore.WebApi.ExtensionMethods
{
    public static class MvcOptionsExtensions
    {
        /// <summary>
        /// Adds the Hypermedia Extensions.
        /// by default a Siren Formatters is added and
        /// the entry assembly is crawled for Hypermedia route attributes
        /// </summary>
        /// <param name="options">The options object of the MVC component.</param>
        /// <param name="alternateRouteRegister">If you wish to use another RoutRegister pass it here, also if you wish another assembly to be crawled.</param>
        /// <param name="alternateQueryStringBuilder">Provide an alternate QueryStringBuilder used for building URL's.</param>
        /// <param name="hypermediaUrlConfig">Configures the URL used in Hypermedia responses.</param>
        /// <param name="hypermediaConverterConfiguration">Configures the creation of Hypermedia documents.</param>
        public static void AddHypermediaExtensions(
            this MvcOptions options,
            IRouteRegister alternateRouteRegister = null,
            IQueryStringBuilder alternateQueryStringBuilder = null,
            IHypermediaUrlConfig hypermediaUrlConfig = null,
            IHypermediaConverterConfiguration hypermediaConverterConfiguration = null)
        {
            var routeRegister = alternateRouteRegister ?? new AttributedRoutesRegister();
            var routeResolverFactory = new RegisterRouteResolverFactory(routeRegister);
            var routeKeyFactory = new RouteKeyFactory(routeRegister);

            var queryStringBuilder = alternateQueryStringBuilder ?? new QueryStringBuilder();
            var hypermediaQueryLocationFormatter = new HypermediaQueryLocationFormatter(routeResolverFactory, routeKeyFactory, queryStringBuilder, hypermediaUrlConfig);
            var hypermediaEntityLocationFormatter = new HypermediaEntityLocationFormatter(routeResolverFactory, routeKeyFactory, hypermediaUrlConfig);

            var sirenHypermediaConverterFactory = new SirenHypermediaConverterFactory(queryStringBuilder, hypermediaConverterConfiguration?.SirenConverterConfiguration);
            var sirenHypermediaFormatter = new SirenHypermediaFormatter(routeResolverFactory, routeKeyFactory, sirenHypermediaConverterFactory, hypermediaUrlConfig);

            options.OutputFormatters.Insert(0, hypermediaQueryLocationFormatter);
            options.OutputFormatters.Insert(0, hypermediaEntityLocationFormatter);
            options.OutputFormatters.Insert(0, sirenHypermediaFormatter);
        }
    }
}
