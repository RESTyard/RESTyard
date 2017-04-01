using Microsoft.AspNetCore.Mvc;

namespace WebApiHypermediaExtensionsCore.WebApi.RouteResolver
{
    public interface IRouteResolverFactory
    {
        IHypermediaRouteResolver CreateRouteResolver(IUrlHelper urlHelper, IRouteKeyFactory routeKeyFactory, HypermediaUrlConfig hypermediaUrlConfig = null);
    }
}