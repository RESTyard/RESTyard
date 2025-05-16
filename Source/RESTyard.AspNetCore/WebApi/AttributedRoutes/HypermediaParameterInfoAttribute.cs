using System;
using Microsoft.AspNetCore.Routing;
using RESTyard.AspNetCore.Hypermedia.Actions;

namespace RESTyard.AspNetCore.WebApi.AttributedRoutes;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class HypermediaParameterInfoAttribute : Attribute, IEndpointNameMetadata
{
    public Type RouteType { get; }
    
    public string EndpointName { get; }

    public HypermediaParameterInfoAttribute(Type routeType)
    {
        AttributedRouteHelper.EnsureIs<IHypermediaActionParameter>(routeType);
        this.RouteType = routeType;
        this.EndpointName = AttributedRouteHelper.EscapeRouteName($"GenericRouteName_ActionParameterInfo_{this.RouteType}");
    }
}