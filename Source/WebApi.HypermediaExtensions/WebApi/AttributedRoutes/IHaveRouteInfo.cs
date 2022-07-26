using System;

namespace RESTyard.WebApi.Extensions.WebApi.AttributedRoutes
{
    public interface IHaveRouteInfo
    {
        Type RouteType { get; }

        Type RouteKeyProducerType { get; }
        
        string AcceptedMediaType { get; }
    }
}