using Newtonsoft.Json.Linq;
using WebApiHypermediaExtensionsCore.Hypermedia;
using WebApiHypermediaExtensionsCore.Query;
using WebApiHypermediaExtensionsCore.WebApi.RouteResolver;

namespace WebApiHypermediaExtensionsCore.WebApi.Formatter
{
    public interface ISirenBuilder
    {
        string CreateSiren(HypermediaObject hypermediaObject, IHypermediaRouteResolver routeResolver, IQueryStringBuilder queryStringBuilder);

        JObject CreateSirenJObject(HypermediaObject hypermediaObject, IHypermediaRouteResolver routeResolver, IQueryStringBuilder queryStringBuilder);
    }
}