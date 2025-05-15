using System;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.AspNetCore.Hypermedia.Links;

namespace RESTyard.AspNetCore.WebApi.RouteResolver
{
    public interface IRouteKeyFactory
    {
        object GetHypermediaRouteKeys(IHypermediaObject hypermediaObject);

        object GetHypermediaRouteKeys(HypermediaObjectReferenceBase reference);

        object GetActionRouteKeys(HypermediaActionBase action, IHypermediaObject actionHostObject);
    }
}