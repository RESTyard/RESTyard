using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using WebApiHypermediaExtensionsCore.Exceptions;
using WebApiHypermediaExtensionsCore.Hypermedia;
using WebApiHypermediaExtensionsCore.Query;
using WebApiHypermediaExtensionsCore.WebApi.RouteResolver;

namespace WebApiHypermediaExtensionsCore.WebApi.Formatter
{
    public class HypermediaQueryLocationFormatter : HypermediaOutputFormatter
    {
        private readonly IQueryStringBuilder queryStringBuilder;

        public HypermediaQueryLocationFormatter(
            IRouteResolverFactory routeResolverFactory,
            IRouteKeyFactory routeKeyFactory,
            IQueryStringBuilder queryStringBuilder,
            IHypermediaUrlConfig defaultHypermediaUrlConfig)
            : base(routeResolverFactory, routeKeyFactory, defaultHypermediaUrlConfig)
        {
            this.queryStringBuilder = queryStringBuilder;
        }

        public override bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            if (context.Object is HypermediaQueryLocation)
            {
                return true;
            }

            return false;
        }

        public override async Task WriteAsync(OutputFormatterWriteContext context)
        {
            var hypermediaQueryLocation = context.Object as HypermediaQueryLocation;
            if (hypermediaQueryLocation == null)
            {
                throw new HypermediaFormatterException($"Formatter expected a {typeof(HypermediaQueryLocation).Name}  but is not.");
            }

            var routeResolver = CreateRouteResolver(context);
            var location = routeResolver.TypeToRoute(hypermediaQueryLocation.QueryType);

            var queryString = queryStringBuilder.CreateQueryString(hypermediaQueryLocation.QueryParameter);
            if (!string.IsNullOrEmpty(queryString))
            {
                location += queryString;
            }

            var response = context.HttpContext.Response;
            response.Headers["Location"] = location;

            response.StatusCode = (int)HttpStatusCode.Created;

            await Task.FromResult(0);
        }
    }
}