using System;
using Microsoft.AspNetCore.Http;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Links;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace RESTyard.AspNetCore.WebApi.Formatter
{
    public class HypermediaEntityLocationFormatter : HypermediaLocationFormatter<HypermediaEntityLocation>, IHypermediaReferenceRouteProvider
    {
        public HypermediaEntityLocationFormatter(
            IRouteResolverFactory routeResolverFactory,
            IRouteKeyFactory routeKeyFactory,
            IHypermediaUrlConfig defaultHypermediaUrlConfig)
            : base(routeResolverFactory, routeKeyFactory, defaultHypermediaUrlConfig)
        {
        }

        protected override void SetResponseValues(HttpResponse response, HypermediaEntityLocation item)
        {
            response.StatusCode = (int) item.HttpStatusCode;
        }

        protected override string GetLocation(IHypermediaRouteResolver routeResolver, HypermediaEntityLocation item)
        {
            return routeResolver.ReferenceToRoute(item.EntityRef).Url;
        }

        protected override HypermediaEntityLocation GetObject(object locationObject)
        {
            return locationObject as HypermediaEntityLocation;
        }

        public string GetRouteUri(HttpContext context, HypermediaObjectReferenceBase referenceBase)
        {
            var routeResolver = this.CreateRouteResolver(context);
            return routeResolver.ReferenceToRoute(referenceBase).Url;
        }
    }
}