using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.WebApi.RouteResolver;

namespace WebApi.HypermediaExtensions.WebApi.Formatter
{
    public class HypermediaEntityLocationFormatter : HypermediaLocationFormatter<HypermediaEntityLocation>
    {
        public HypermediaEntityLocationFormatter(IRouteResolverFactory routeResolverFactory,
            IRouteKeyFactory routeKeyFactory,
            IHypermediaUrlConfig defaultHypermediaUrlConfig)
            : base(routeResolverFactory, routeKeyFactory, defaultHypermediaUrlConfig)
        {
        }

        protected override void SetResponseValues(HttpResponse response, HypermediaEntityLocation item)
        {
            response.StatusCode = (int) item.HttpStatusCode;
        }

        protected override StringValues GetLocation(IHypermediaRouteResolver routeResolver, HypermediaEntityLocation item)
        {
            return routeResolver.ReferenceToRoute(item.EntityRef);
        }

        protected override HypermediaEntityLocation GetObject(object locationObject)
        {
            return locationObject as HypermediaEntityLocation;
        }
    }
}