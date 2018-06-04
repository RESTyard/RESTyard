using Microsoft.AspNetCore.Mvc;

namespace WebApi.HypermediaExtensions.WebApi.RouteResolver
{
    public interface IRouteResolverFactory
    {
        IHypermediaRouteResolver CreateRouteResolver(IUrlHelper urlHelper, IRouteKeyFactory routeKeyFactory, HypermediaUrlConfig hypermediaUrlConfig = null);
    }
}