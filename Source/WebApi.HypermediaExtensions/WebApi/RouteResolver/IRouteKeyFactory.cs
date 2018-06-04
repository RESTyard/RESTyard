using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.Hypermedia.Actions;
using WebApi.HypermediaExtensions.Hypermedia.Links;

namespace WebApi.HypermediaExtensions.WebApi.RouteResolver
{
    public interface IRouteKeyFactory
    {
        object GetHypermediaRouteKeys(HypermediaObject hypermediaObject);

        object GetHypermediaRouteKeys(HypermediaObjectReferenceBase reference);

        object GetActionRouteKeys(HypermediaActionBase action, HypermediaObject actionHostObject);
    }
}