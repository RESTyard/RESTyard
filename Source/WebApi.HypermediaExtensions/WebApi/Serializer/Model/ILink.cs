using FunicularSwitch;
using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.WebApi.RouteResolver;

namespace WebApi.HypermediaExtensions.WebApi.Serializer.Model
{
    public interface ILink
    {
        Result<string> ResolveUrl(IHypermediaRouteResolver resolver, object hypermediaObject);

    }
}