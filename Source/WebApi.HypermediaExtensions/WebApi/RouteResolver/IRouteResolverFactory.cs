using System;
using Microsoft.AspNetCore.Mvc;

namespace RESTyard.AspNetCore.WebApi.RouteResolver
{
    public interface IRouteResolverFactory
    {
        IHypermediaRouteResolver CreateRouteResolver(IUrlHelper urlHelper, IRouteKeyFactory routeKeyFactory, IHypermediaUrlConfig hypermediaUrlConfig = null);
    }
}