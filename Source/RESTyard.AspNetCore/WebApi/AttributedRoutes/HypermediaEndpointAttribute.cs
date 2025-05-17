using System;
using System.Reflection;
using FunicularSwitch.Extensions;
using Microsoft.AspNetCore.Routing;
using RESTyard.AspNetCore.Exceptions;
using RESTyard.AspNetCore.Hypermedia.Attributes;

namespace RESTyard.AspNetCore.WebApi.AttributedRoutes;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class HypermediaEndpointAttribute : Attribute, IEndpointNameMetadata
{
    public Type RouteType { get; }
    public Type? RouteKeyProducerType { get; }
        
    public string? AcceptedMediaType { get; }
    
    public string EndpointName { get; }

    
    public Type? ActionType { get; }

    public HypermediaEndpointAttribute(Type routeType, Type? routeKeyProducerType = null)
    {
        this.RouteType = routeType;
        this.RouteKeyProducerType = routeKeyProducerType;
        AttributedRouteHelper.EnsureHas<HypermediaObjectAttribute>(this.RouteType);
        AttributedRouteHelper.EnsureIsRouteKeyProducer(routeKeyProducerType);
        this.EndpointName = AttributedRouteHelper.EscapeRouteName($"GenericRouteName_HypermediaObject_{this.RouteType}");
    }

    public HypermediaEndpointAttribute(Type routeType, string actionPropertyName, string? acceptedMediaType = null) : this(routeType)
    {
        this.AcceptedMediaType = acceptedMediaType;
        var property = this.RouteType.GetProperty(actionPropertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property is null)
        {
            throw new HypermediaException(
                $"Property '{actionPropertyName}' not found on type {this.RouteType.BeautifulName()}");
        }

        this.ActionType = property.PropertyType;
        this.EndpointName = AttributedRouteHelper.EscapeRouteName($"GenericRouteName_HypermediaAction_{this.ActionType}");
    }
}