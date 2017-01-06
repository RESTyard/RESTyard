using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using WebApiHypermediaExtensionsCore.Query;
using WebApiHypermediaExtensionsCore.WebApi.RouteResolver;

namespace WebApiHypermediaExtensionsCore.WebApi.Formatter
{
    public abstract class HypermediaOutputFormatter : IOutputFormatter
    {
        protected readonly IRouteResolverFactory RouteResolverFactory;
        protected readonly IRouteKeyFactory RouteKeyFactory;
        protected readonly IQueryStringBuilder QueryStringBuilder;
        protected readonly HypermediaUrlConfig DefaultHypermediaUrlConfig;

        protected HypermediaOutputFormatter(
            IRouteResolverFactory routeResolverFactory,
            IRouteKeyFactory routeKeyFactory,
            IQueryStringBuilder queryStringBuilder,
            HypermediaUrlConfig defaultHypermediaUrlConfig = null)
        {
            RouteResolverFactory = routeResolverFactory;
            RouteKeyFactory = routeKeyFactory;
            QueryStringBuilder = queryStringBuilder;
            DefaultHypermediaUrlConfig = defaultHypermediaUrlConfig ?? new HypermediaUrlConfig();
        }

        protected virtual IHypermediaRouteResolver CreateRouteResolver(OutputFormatterWriteContext context)
        {
            var hypermediaUrlConfig = new HypermediaUrlConfig(DefaultHypermediaUrlConfig, context.HttpContext.Request);
            var urlHelper = FormatterHelper.GetUrlHelperForCurrentContext(context);
            var routeResolver = RouteResolverFactory.CreateRouteResolver(urlHelper, RouteKeyFactory, hypermediaUrlConfig);
            return routeResolver;
        }

        public abstract bool CanWriteResult(OutputFormatterCanWriteContext context);
        public abstract Task WriteAsync(OutputFormatterWriteContext context);
    }
}