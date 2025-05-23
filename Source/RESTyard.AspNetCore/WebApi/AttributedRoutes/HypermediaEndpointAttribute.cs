using System;
using Microsoft.AspNetCore.Routing;

namespace RESTyard.AspNetCore.WebApi.AttributedRoutes;

public interface IHypermediaEndpointMetadata : IEndpointNameMetadata
{
    Type RouteType { get; }
}

public abstract class HypermediaEndpointAttribute : Attribute, IHypermediaEndpointMetadata
{
    protected HypermediaEndpointAttribute(Type routeType)
    {
        this.RouteType = routeType;
    }
 
    public Type RouteType { get; }
    
    public abstract string EndpointName { get; }
}