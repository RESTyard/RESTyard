using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Primitives;
using WebApiHypermediaExtensionsCore.Hypermedia;
using WebApiHypermediaExtensionsCore.WebApi.RouteResolver;

namespace WebApiHypermediaExtensionsCore.WebApi.Formatter
{
    public class HypermediaEntityLocationFormatter : HypermediaLocationFormatter<HypermediaEntityLocation>
    {
        public HypermediaEntityLocationFormatter(IRouteResolverFactory routeResolverFactory,
            IRouteKeyFactory routeKeyFactory) : base(routeResolverFactory, routeKeyFactory)
        {
        }

        public override bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            if (context.Object is HypermediaEntityLocation)
            {
                return true;
            }

            return false;
        }

        protected override void SetResponseValues(HttpResponse response, HypermediaEntityLocation item)
        {
            response.StatusCode = (int) item.HttpStatusCode;
        }

        protected override StringValues GetLocation(IHypermediaRouteResolver routeResolver,
            HypermediaEntityLocation item)
        {
            return routeResolver.ReferenceToRoute(item.EntityRef);
        }

        protected override HypermediaEntityLocation GetObject(object locationObject)
        {
            return locationObject as HypermediaEntityLocation;
        }
    }
}