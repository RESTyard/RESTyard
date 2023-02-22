using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RESTyard.AspNetCore.WebApi.RouteResolver
{
    public interface IRouteResolverFactory
    {
        IHypermediaRouteResolver CreateRouteResolver(HttpContext httpContext);

        IHypermediaRouteResolver CreateRouteResolver(IUrlHelper urlHelper);
    }
}