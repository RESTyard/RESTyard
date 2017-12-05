namespace WebApiHypermediaExtensionsCore.WebApi.RouteResolver
{
    using Hypermedia;
    using Hypermedia.Links;

    using WebApiHypermediaExtensionsCore.Hypermedia.Actions;

    public interface IRouteKeyFactory
    {
        object GetHypermediaRouteKeys(HypermediaObject hypermediaObject);

        object GetHypermediaRouteKeys(HypermediaObjectReferenceBase reference);

        object GetActionRouteKeys(HypermediaActionBase action, HypermediaObject actionHostObject);
    }
}