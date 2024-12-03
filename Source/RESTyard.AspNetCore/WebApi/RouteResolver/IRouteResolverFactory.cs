using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace RESTyard.AspNetCore.WebApi.RouteResolver
{
    public interface IRouteResolverFactory
    {
        IHypermediaRouteResolver CreateRouteResolver(HttpContext httpContext);

        IHypermediaRouteResolver CreateRouteResolver(IUrlHelper urlHelper, IHypermediaUrlConfig urlConfig);
    }

    public interface IRouteResolverFactory2
    {
        IHypermediaRouteResolver CreateRouteResolver(HttpContext httpContext, LinkGenerator linkGenerator);
    }
}