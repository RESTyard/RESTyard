using System;
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

        bool TryGetRouteByType(Type type, out ResolvedRoute route, object routeKeys = null);

        string RouteUrl(string routeName, object routeKeys = null);
        
        ResolvedRoute RouteUrl(RouteInfo routeInfo, object routeKeys = null);
    }
}