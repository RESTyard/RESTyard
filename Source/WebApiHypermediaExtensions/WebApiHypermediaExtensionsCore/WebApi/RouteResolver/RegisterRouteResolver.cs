using System;
using Microsoft.AspNetCore.Mvc;
using WebApiHypermediaExtensionsCore.Exceptions;
using WebApiHypermediaExtensionsCore.Hypermedia;
using WebApiHypermediaExtensionsCore.Hypermedia.Actions;
using WebApiHypermediaExtensionsCore.Hypermedia.Links;

namespace WebApiHypermediaExtensionsCore.WebApi.RouteResolver
{
    public class RegisterRouteResolver : IHypermediaRouteResolver
    {
        protected IRouteRegister RouteRegister;
        private readonly HypermediaUrlConfig hypermediaUrlConfig;

        private readonly IUrlHelper urlHelper;

        private readonly IRouteKeyFactory routeKeyFactory;

        public RegisterRouteResolver(IUrlHelper urlHelper, IRouteKeyFactory routeKeyFactory, IRouteRegister routeRegister, HypermediaUrlConfig hypermediaUrlConfig = null)
        {
            this.RouteRegister = routeRegister;
            this.urlHelper = urlHelper;
            this.hypermediaUrlConfig = hypermediaUrlConfig ?? new HypermediaUrlConfig();
            this.routeKeyFactory = routeKeyFactory;
        }

        public string ObjectToRoute(HypermediaObject hypermediaObject)
        {
            var lookupType = hypermediaObject.GetType();
            var routeKeys = this.routeKeyFactory.GetHypermediaRouteKeys(hypermediaObject);
            return this.GetRouteByType(lookupType, routeKeys);
        }

        public string ReferenceToRoute(HypermediaObjectReferenceBase reference)
        {
            var lookupType = reference.GetHypermediaType();
            var routeKeys = this.routeKeyFactory.GetHypermediaRouteKeys(reference);
            return this.GetRouteByType(lookupType, routeKeys);
        }

        public string ActionToRoute(HypermediaObject hypermediaObject, HypermediaActionBase action)
        {
            var lookupType = action.GetType();
            var routeKeys = routeKeyFactory.GetHypermediaRouteKeys(hypermediaObject);
            return this.GetRouteByType(lookupType, routeKeys);
        }

        public string TypeToRoute(Type type)
        {
            return this.GetRouteByType(type);
        }

        private string GetRouteByType(Type lookupType, object routeKeys = null)
        {
            var routeName = this.RouteRegister.GetRoute(lookupType);
            var route = this.urlHelper.RouteUrl(routeName, routeKeys, hypermediaUrlConfig.Scheme, hypermediaUrlConfig.Host.ToUriComponent());

            if (route == null)
            {
                throw new RouteResolverException($"Could not build route: '{routeName}'");
            }

            return route;
        }
    }
}