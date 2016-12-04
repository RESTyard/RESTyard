namespace WebApiHypermediaExtensionsCore.WebApi.RouteResolver
{
    using Hypermedia;
    using Hypermedia.Links;

    public interface IRouteKeyFactory
    {
        object GetHypermediaRouteKeys(HypermediaObject hypermediaObject);
        object GetHypermediaRouteKeys(HypermediaObjectReference reference);
        object GetHypermediaRouteKeys(HypermediaObjectKeyReference reference);
        object GetHypermediaRouteKeys(HypermediaObjectReferenceBase reference);
    }
}