using System;

namespace RESTyard.AspNetCore.WebApi.AttributedRoutes
{
    public interface IHaveRouteInfo
    {
        Type RouteType { get; }

        Type RouteKeyProducerType { get; }
        
        string AcceptedMediaType { get; }
    }
}