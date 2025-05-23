using System;
using System.Reflection;
using FunicularSwitch.Extensions;
using RESTyard.AspNetCore.Exceptions;
using RESTyard.AspNetCore.Hypermedia.Attributes;

namespace RESTyard.AspNetCore.WebApi.AttributedRoutes;

public interface IHypermediaActionEndpointMetadata : IHypermediaEndpointMetadata
{
    string? AcceptedMediaType { get; }
    Type ActionType { get; }
}

[AttributeUsage(AttributeTargets.Method)]
public class HypermediaActionEndpointAttribute<THto> : HypermediaEndpointAttribute, IHypermediaActionEndpointMetadata
{
    public HypermediaActionEndpointAttribute(string actionPropertyName, string? acceptedMediaType = null) : base(typeof(THto))
    {
        this.AcceptedMediaType = acceptedMediaType;
        AttributedRouteHelper.EnsureHas<HypermediaObjectAttribute>(this.RouteType);
        var property = this.RouteType.GetProperty(actionPropertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property is null)
        {
            throw new HypermediaException(
                $"Property '{actionPropertyName}' not found on type {this.RouteType.BeautifulName()}");
        }

        this.ActionType = property.PropertyType;
        this.EndpointName = AttributedRouteHelper.EscapeRouteName($"GenericRouteName_HypermediaAction_{this.ActionType}");
    }
    
    public string? AcceptedMediaType { get; }
    public Type? ActionType { get; }
    public override string EndpointName { get; }
}