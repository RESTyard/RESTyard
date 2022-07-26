using System;

namespace WebApi.HypermediaExtensions.WebApi.AttributedRoutes
{
    public interface IHaveRouteInfo
    {
        Type RouteType { get; }

        Type RouteKeyProducerType { get; }
        
        string AcceptedMediaType { get; }
    }
}