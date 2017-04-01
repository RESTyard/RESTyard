using System;
using WebApiHypermediaExtensionsCore.Hypermedia;
using WebApiHypermediaExtensionsCore.Hypermedia.Actions;
using WebApiHypermediaExtensionsCore.Hypermedia.Links;

namespace WebApiHypermediaExtensionsCore.WebApi.RouteResolver
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
    }
}