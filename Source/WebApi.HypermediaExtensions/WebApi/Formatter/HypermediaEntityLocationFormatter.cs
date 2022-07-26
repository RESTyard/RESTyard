using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using RESTyard.WebApi.Extensions.Hypermedia;
using RESTyard.WebApi.Extensions.WebApi.RouteResolver;

namespace RESTyard.WebApi.Extensions.WebApi.Formatter
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
            return routeResolver.ReferenceToRoute(item.EntityRef).Url;
        }

        protected override HypermediaEntityLocation GetObject(object locationObject)
        {
            return locationObject as HypermediaEntityLocation;
        }
    }
}