using System;
using Microsoft.AspNetCore.Routing;
using RESTyard.AspNetCore.Hypermedia.Actions;

namespace RESTyard.AspNetCore.WebApi.AttributedRoutes;

public interface IHypermediaActionParameterInfoEndpointMetadata : IHypermediaEndpointMetadata;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class HypermediaActionParameterInfoEndpointAttribute<TParameter> : HypermediaEndpointAttribute, IHypermediaActionParameterInfoEndpointMetadata
    where TParameter : IHypermediaActionParameter
{
    public override string EndpointName { get; }

    public HypermediaActionParameterInfoEndpointAttribute() : base(typeof(TParameter))
    {
        this.EndpointName = AttributedRouteHelper.EscapeRouteName($"GenericRouteName_ActionParameterInfo_{this.RouteType}");
    }
}