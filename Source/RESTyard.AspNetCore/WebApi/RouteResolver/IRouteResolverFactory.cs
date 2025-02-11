using Microsoft.AspNetCore.Http;

namespace RESTyard.AspNetCore.WebApi.RouteResolver
{
    public interface IRouteResolverFactory
    {
        IHypermediaRouteResolver CreateRouteResolver(HttpContext httpContext, IHypermediaUrlConfig? urlConfig = null);
    }
}