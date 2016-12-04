using System;
using WebApiHypermediaExtensionsCore.Hypermedia;
using WebApiHypermediaExtensionsCore.Hypermedia.Actions;
using WebApiHypermediaExtensionsCore.Hypermedia.Links;

namespace WebApiHypermediaExtensionsCore.WebApi.RouteResolver
{
    // maps hypermedia object to a accessible webapi route, used by the formatter
    public interface IHypermediaRouteResolver
    {
        string ObjectToRoute(HypermediaObject hypermediaObject);

        string ReferenceToRoute(HypermediaObjectReferenceBase reference);

        string ActionToRoute(HypermediaObject hypermediaObject, HypermediaActionBase reference);
        
        // Actions with parameters require a route to a type, this route can hold a json schema
        string ParameterTypeToRoute(Type actionParameterType);
    }
}