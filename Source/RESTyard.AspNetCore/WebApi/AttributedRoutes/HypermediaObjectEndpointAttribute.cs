using System;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Attributes;

namespace RESTyard.AspNetCore.WebApi.AttributedRoutes;

public interface IHypermediaObjectEndpointMetadata : IHypermediaEndpointMetadata
{
    Type? RouteKeyProducerType { get; }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class HypermediaObjectEndpointAttribute<THto> : HypermediaEndpointAttribute, IHypermediaObjectEndpointMetadata
    where THto : IHypermediaObject
{
    public Type? RouteKeyProducerType { get; }
    
    public override string EndpointName { get; }
 
    public HypermediaObjectEndpointAttribute(Type? routeKeyProducerType = null) : base(typeof(THto))
    {
        this.RouteKeyProducerType = routeKeyProducerType;
        AttributedRouteHelper.EnsureHas<HypermediaObjectAttribute>(this.RouteType);
        AttributedRouteHelper.EnsureIsRouteKeyProducer(routeKeyProducerType);
        this.EndpointName = AttributedRouteHelper.EscapeRouteName($"GenericRouteName_HypermediaObject_{this.RouteType}");
    }
}