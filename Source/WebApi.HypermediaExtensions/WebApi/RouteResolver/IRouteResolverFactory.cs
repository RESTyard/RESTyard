using System;
using Microsoft.AspNetCore.Mvc;

namespace RESTyard.WebApi.Extensions.WebApi.RouteResolver
{
    public interface IRouteResolverFactory
    {
        IHypermediaRouteResolver CreateRouteResolver(IUrlHelper urlHelper, IRouteKeyFactory routeKeyFactory, IHypermediaUrlConfig hypermediaUrlConfig = null);
    }
}