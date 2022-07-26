using System;
using RESTyard.WebApi.Extensions.WebApi.RouteResolver;

namespace RESTyard.WebApi.Extensions.WebApi.AttributedRoutes
{
    public interface IHaveRouteKeyProducer
    {
        IKeyProducer RouteKeyProducer { get; }
    }
}