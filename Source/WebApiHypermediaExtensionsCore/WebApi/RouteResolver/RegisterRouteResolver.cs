using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using WebApiHypermediaExtensionsCore.Exceptions;
using WebApiHypermediaExtensionsCore.Hypermedia;
using WebApiHypermediaExtensionsCore.Hypermedia.Actions;
using WebApiHypermediaExtensionsCore.Hypermedia.Links;
using WebApiHypermediaExtensionsCore.WebApi.ExtensionMethods;

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
        private readonly bool returnDefaultRouteForUnknownHto;
        private readonly string defaultRouteSegmentForUnknownHto;

        public RegisterRouteResolver(IUrlHelper urlHelper, IRouteKeyFactory routeKeyFactory, IRouteRegister routeRegister, HypermediaExtensionsOptions hypermediaOptions, HypermediaUrlConfig hypermediaUrlConfig = null)
        {
            this.RouteRegister = routeRegister;
            this.urlHelper = urlHelper;
            this.hypermediaUrlConfig = hypermediaUrlConfig ?? new HypermediaUrlConfig();
            this.routeKeyFactory = routeKeyFactory;
            this.returnDefaultRouteForUnknownHto = hypermediaOptions.ReturnDefaultRouteForUnknownHto;
            this.defaultRouteSegmentForUnknownHto = hypermediaOptions.DefaultRouteSegmentForUnknownHto;
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
            if (typeof(ExternalReference).GetTypeInfo().IsAssignableFrom(lookupType))
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

        public bool TryGetRouteByType(Type type, out string route, object routeKeys = null)
        {
            string routeName;
            route = null;
            if (this.RouteRegister.TryGetRoute(type, out routeName))
            {
                route = this.urlHelper.RouteUrl(routeName, routeKeys, hypermediaUrlConfig.Scheme, hypermediaUrlConfig.Host.ToUriComponent());
            }
            return route != null;
        }

        private string GetRouteByType(Type lookupType, object routeKeys = null)
        {
            var foundRoute = this.RouteRegister.TryGetRoute(lookupType, out var routeName);
            if (!foundRoute)
            {
                return this.HandleUnknownRoute(lookupType);
            }


            var route = this.urlHelper.RouteUrl(routeName, routeKeys, hypermediaUrlConfig.Scheme, hypermediaUrlConfig.Host.ToUriComponent());
            if (route == null)
            {
                throw new RouteResolverException($"Could not build route: '{routeName}'");
            }

            return route;
        }

        private string HandleUnknownRoute(Type lookupType)
        {
            if (returnDefaultRouteForUnknownHto)
            {
                return $"{hypermediaUrlConfig.Scheme}://{hypermediaUrlConfig.Host.ToUriComponent()}/{defaultRouteSegmentForUnknownHto}";
            }

            throw new RouteResolverException($"Route to type '{lookupType.Name}' not found in RouteRegister.");
        }
    }
}