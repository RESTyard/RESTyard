using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using WebApiHypermediaExtensionsCore.Exceptions;
using WebApiHypermediaExtensionsCore.Hypermedia;
using WebApiHypermediaExtensionsCore.Hypermedia.Actions;
using WebApiHypermediaExtensionsCore.Hypermedia.Links;

namespace WebApiHypermediaExtensionsCore.WebApi.RouteResolver
{
    /// <summary>
    /// Uses a IRouteRegister to access routes by type.
    /// Handles <see cref="ExternalReference"/> for ReferenceToRoute.
    /// </summary>
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

            // ExternalReference object is not registered in the RouteRegister and provides its own URI
            if (typeof(ExternalReference).IsAssignableFrom(lookupType))
            {
                var externalReferenceObject = reference.GetInstance() as ExternalReference;
                if (externalReferenceObject == null)
                {
                    throw new HypermediaRouteException("Can not get instance for ExternalReference.");
                }

                return externalReferenceObject.ExternalUri.ToString();
            }

            var routeKeys = this.routeKeyFactory.GetHypermediaRouteKeys(reference);
            return this.GetRouteByType(lookupType, routeKeys);
        }

        public string ActionToRoute(HypermediaObject actionHostObject, HypermediaActionBase action)
        {
            var lookupType = action.GetType();
            var routeKeys = routeKeyFactory.GetActionRouteKeys(action, actionHostObject);
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