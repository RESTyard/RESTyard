using System;

namespace WebApiHypermediaExtensionsCore.WebApi.AttributedRoutes
{
    public interface IHaveRouteInfo
    {
        Type RouteType { get; }

        Type RouteKeyProducerType { get; }
    }
}