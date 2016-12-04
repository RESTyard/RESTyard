using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using WebApiHypermediaExtensionsCore.Exceptions;
using WebApiHypermediaExtensionsCore.Hypermedia;
using WebApiHypermediaExtensionsCore.Query;
using WebApiHypermediaExtensionsCore.WebApi.RouteResolver;

namespace WebApiHypermediaExtensionsCore.WebApi.Formatter
{
    public class HypermediaQueryLocationFormatter : IOutputFormatter {
        private readonly IRouteResolverFactory routeResolverFactory;
        private readonly IRouteKeyFactory routeKeyFactory;
        private readonly IQueryStringBuilder queryStringBuilder;

        public HypermediaQueryLocationFormatter(IRouteResolverFactory routeResolverFactory, IRouteKeyFactory routeKeyFactory, IQueryStringBuilder queryStringBuilder)
        {
            this.routeResolverFactory = routeResolverFactory;
            this.routeKeyFactory = routeKeyFactory;
            this.queryStringBuilder = queryStringBuilder;
        }

        public bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            if (context.Object is HypermediaQueryLocation)
            {
                return true;
            }

            return false;
        }

        public async Task WriteAsync(OutputFormatterWriteContext context)
        {
            var hypermediaQueryLocation = context.Object as HypermediaQueryLocation;
            if (hypermediaQueryLocation == null)
            {
                throw new HypermediaFormatterException($"Formatter expected a {typeof(HypermediaQueryLocation).Name}  but is not.");
            }

            var urlHelper = FormatterHelper.GetUrlHelperForCurrentContext(context);

            var routeResolver = this.routeResolverFactory.CreateRouteResolver(urlHelper, this.routeKeyFactory);

            var location = routeResolver.ParameterTypeToRoute(hypermediaQueryLocation.QueryType);

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