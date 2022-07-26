using System;
using RESTyard.WebApi.Extensions.Hypermedia;
using RESTyard.WebApi.Extensions.Hypermedia.Actions;
using RESTyard.WebApi.Extensions.Hypermedia.Links;

namespace RESTyard.WebApi.Extensions.WebApi.RouteResolver
{
    public interface IRouteKeyFactory
    {
        object GetHypermediaRouteKeys(HypermediaObject hypermediaObject);

        object GetHypermediaRouteKeys(HypermediaObjectReferenceBase reference);

        object GetActionRouteKeys(HypermediaActionBase action, HypermediaObject actionHostObject);
    }
}