using System;
using System.Diagnostics.CodeAnalysis;
using FunicularSwitch;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.AspNetCore.Hypermedia.Links;

namespace RESTyard.AspNetCore.WebApi.RouteResolver
{
    /// <summary>
    /// Maps hypermedia objects to a route.
    /// </summary>
    public interface IHypermediaRouteResolver
    {
        ResolvedRoute ObjectToRoute(HypermediaObject hypermediaObject);

        ResolvedRoute ReferenceToRoute(HypermediaObjectReferenceBase reference);

        ResolvedRoute ActionToRoute(HypermediaObject hypermediaObject, HypermediaActionBase reference);

        ResolvedRoute TypeToRoute(Type actionParameterType);

        Option<ResolvedRoute> TryGetRouteByType(Type type, object? routeKeys = null);

        Result<string> RouteUrl(string routeName, object? routeKeys = null);
        
        Result<ResolvedRoute> RouteUrl(RouteInfo routeInfo, object? routeKeys = null);
    }
}