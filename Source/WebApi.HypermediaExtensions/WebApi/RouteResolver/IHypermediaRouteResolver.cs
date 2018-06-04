using System;
using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.Hypermedia.Actions;
using WebApi.HypermediaExtensions.Hypermedia.Links;

namespace WebApi.HypermediaExtensions.WebApi.RouteResolver
{
    /// <summary>
    /// Maps hypermedia objects to a route.
    /// </summary>
    public interface IHypermediaRouteResolver
    {
        string ObjectToRoute(HypermediaObject hypermediaObject);

        string ReferenceToRoute(HypermediaObjectReferenceBase reference);

        string ActionToRoute(HypermediaObject hypermediaObject, HypermediaActionBase reference);
        
        string TypeToRoute(Type actionParameterType);
        bool TryGetRouteByType(Type type, out string route, object routeKeys = null);
    }
}