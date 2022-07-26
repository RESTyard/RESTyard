using System;
using RESTyard.WebApi.Extensions.Hypermedia;
using RESTyard.WebApi.Extensions.Hypermedia.Actions;
using RESTyard.WebApi.Extensions.Hypermedia.Links;

namespace RESTyard.WebApi.Extensions.WebApi.RouteResolver
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