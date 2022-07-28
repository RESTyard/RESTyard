using System;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace RESTyard.AspNetCore.WebApi.AttributedRoutes
{
    public interface IHaveRouteKeyProducer
    {
        IKeyProducer RouteKeyProducer { get; }
    }
}