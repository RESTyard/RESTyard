namespace WebApiHypermediaExtensionsCore.WebApi.RouteResolver
{
    using Hypermedia;
    using Hypermedia.Links;

    public interface IRouteKeyFactory
    {
        object GetHypermediaRouteKeys(HypermediaObject hypermediaObject);
        object GetHypermediaRouteKeys(HypermediaObjectReferenceBase reference);
    }
}