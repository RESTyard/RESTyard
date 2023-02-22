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

        protected HypermediaOutputFormatter(
            IRouteResolverFactory routeResolverFactory)
        {
            RouteResolverFactory = routeResolverFactory;
        }

        protected virtual IHypermediaRouteResolver CreateRouteResolver(HttpContext httpContext)
        {
            var routeResolver = RouteResolverFactory.CreateRouteResolver(httpContext);
            return routeResolver;
        }

        public abstract bool CanWriteResult(OutputFormatterCanWriteContext context);
        public abstract Task WriteAsync(OutputFormatterWriteContext context);
    }
}