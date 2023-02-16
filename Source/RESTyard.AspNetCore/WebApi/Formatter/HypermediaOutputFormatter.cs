using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace RESTyard.AspNetCore.WebApi.Formatter
{
    public abstract class HypermediaOutputFormatter : IOutputFormatter
    {
        protected readonly IRouteResolverFactory RouteResolverFactory;
        protected readonly IRouteKeyFactory RouteKeyFactory;
        protected readonly IHypermediaUrlConfig DefaultHypermediaUrlConfig;

        protected HypermediaOutputFormatter(
            IRouteResolverFactory routeResolverFactory,
            IRouteKeyFactory routeKeyFactory,
            IHypermediaUrlConfig defaultHypermediaUrlConfig = null)
        {
            RouteResolverFactory = routeResolverFactory;
            RouteKeyFactory = routeKeyFactory;
            DefaultHypermediaUrlConfig = defaultHypermediaUrlConfig ?? new HypermediaUrlConfig();
        }

        protected virtual IHypermediaRouteResolver CreateRouteResolver(HttpContext httpContext)
        {
            var hypermediaUrlConfig = new HypermediaUrlConfigBuilder(DefaultHypermediaUrlConfig, httpContext.Request);
            var urlHelper = FormatterHelper.GetUrlHelperForCurrentContext(httpContext);
            var routeResolver = RouteResolverFactory.CreateRouteResolver(urlHelper, RouteKeyFactory, hypermediaUrlConfig);
            return routeResolver;
        }

        public abstract bool CanWriteResult(OutputFormatterCanWriteContext context);
        public abstract Task WriteAsync(OutputFormatterWriteContext context);
    }
}