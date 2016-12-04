using WebApiHypermediaExtensionsCore.Hypermedia;

namespace WebApiHypermediaExtensionsCore.WebApi.RouteResolver
{
    /// <summary>
    /// Derive from this interface to generate a RouteKeyProducer for an <see cref="HypermediaObject"/>. 
    /// When building routes to HypermediaObjects the RouteResolver will try instanciate a RouteKeyProducer if provided in the route attributes and call GetKey.
    /// There can only be a constructor without parameters.
    /// </summary>
    public interface IRouteKeyProducer
    {
        /// <summary>
        /// Must generate a anonymous object which is passed to an UrlHelper which generates a Route for a HypermediaObject. The <see cref="HypermediaObject"/> for which the route is build will be passed.
        /// The anonymous object must contain a field which is named exactly as the key in the route template so the UrlHelper matches.
        /// </summary>
        /// <param name="hypermediaObject"></param>
        /// <returns></returns>
        object GetKey(object hypermediaObject);
    }
}