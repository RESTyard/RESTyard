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
        public static void AddHypermediaExtensions(this MvcOptions options, IRouteRegister alternateRouteRegister = null, IQueryStringBuilder alternateQueryStringBuilder = null)
        {
            var routeRegister = alternateRouteRegister ?? new AttributedRoutesRegister();
            var queryStringBuilder = alternateQueryStringBuilder ?? new QueryStringBuilder();

            var routeResolverFactory = new RegisterRouteResolverFactory(routeRegister);
            var routeKeyFactory = new RouteKeyFactory(routeRegister);

            var hypermediaQueryLocationFormatter = new HypermediaQueryLocationFormatter(routeResolverFactory, routeKeyFactory, queryStringBuilder);
            var hypermediaEntityLocationFormatter = new HypermediaEntityLocationFormatter(routeResolverFactory, routeKeyFactory);
            var sirenHypermediaFormatter = new SirenHypermediaFormatter(routeResolverFactory, routeKeyFactory, queryStringBuilder);

            options.OutputFormatters.Insert(0, hypermediaQueryLocationFormatter);
            options.OutputFormatters.Insert(0, hypermediaEntityLocationFormatter);
            options.OutputFormatters.Insert(0, sirenHypermediaFormatter);
        }
    }
}
