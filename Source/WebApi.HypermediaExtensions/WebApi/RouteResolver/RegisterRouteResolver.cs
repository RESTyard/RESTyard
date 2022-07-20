using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using WebApi.HypermediaExtensions.Exceptions;
using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.Hypermedia.Actions;
using WebApi.HypermediaExtensions.Hypermedia.Links;
using WebApi.HypermediaExtensions.WebApi.ExtensionMethods;

namespace WebApi.HypermediaExtensions.WebApi.RouteResolver
{
    /// <summary>
    /// Uses a IRouteRegister to access routes by type.
    /// Handles <see cref="ExternalReference"/> for ReferenceToRoute.
    /// </summary>
    public class RegisterRouteResolver : IHypermediaRouteResolver
    {
        protected IRouteRegister RouteRegister;

        private readonly IHypermediaUrlConfig hypermediaUrlConfig;

        private readonly IUrlHelper urlHelper;

        private readonly IRouteKeyFactory routeKeyFactory;
        private readonly bool returnDefaultRouteForUnknownHto;
        private readonly string defaultRouteSegmentForUnknownHto;
        private readonly TypeInfo externalReferenceTypeInfo = typeof(ExternalReference).GetTypeInfo();
        private readonly TypeInfo internalReferenceTypeInfo = typeof(InternalReference).GetTypeInfo();

        public RegisterRouteResolver(IUrlHelper urlHelper, IRouteKeyFactory routeKeyFactory, IRouteRegister routeRegister, HypermediaExtensionsOptions hypermediaOptions, IHypermediaUrlConfig hypermediaUrlConfig = null)
        {
            this.RouteRegister = routeRegister;
            this.urlHelper = urlHelper;
            this.hypermediaUrlConfig = hypermediaUrlConfig ?? new HypermediaUrlConfig();
            this.routeKeyFactory = routeKeyFactory;
            this.returnDefaultRouteForUnknownHto = hypermediaOptions.ReturnDefaultRouteForUnknownHto;
            this.defaultRouteSegmentForUnknownHto = hypermediaOptions.DefaultRouteSegmentForUnknownHto;
        }

        public ResolvedRoute ObjectToRoute(HypermediaObject hypermediaObject)
        {
            var lookupType = hypermediaObject.GetType();
            var routeKeys = this.routeKeyFactory.GetHypermediaRouteKeys(hypermediaObject);
            return this.GetRouteByType(lookupType, routeKeys);
        }

        public ResolvedRoute ReferenceToRoute(HypermediaObjectReferenceBase reference)
        {
            var lookupType = reference.GetHypermediaType();

            // ExternalReference object is not registered in the RouteRegister and provides its own URI
            
            if (externalReferenceTypeInfo.IsAssignableFrom(lookupType))
            {
                if (!(reference.GetInstance() is ExternalReference externalReferenceObject))
                {
                    throw new HypermediaRouteException("Can not get instance for ExternalReference.");
                }

                // we assume get here since external references will be links only for now
                return new ResolvedRoute(externalReferenceObject.ExternalUri.ToString(), HttpMethod.GET, externalReferenceObject.AvailableMediaTypes);
            }     
            
            if (internalReferenceTypeInfo.IsAssignableFrom(lookupType))
            {
                if (!(reference.GetInstance() is InternalReference internalReference))
                {
                    throw new HypermediaRouteException("Can not get instance for InternalReference.");
                }

                // we assume get here since external references will be links only for now
                var routeInfo = new RouteInfo(internalReference.RouteName, HttpMethod.GET);
                var resolvedInternalRoute = RouteUrl(routeInfo, internalReference.RouteParameters);
                resolvedInternalRoute.AvailableMediaTypes = internalReference.AvailableMediaTypes;
                return resolvedInternalRoute;
            }

            if (reference is HypermediaExternalObjectReference)
            {
                throw new HypermediaRouteException("Can not get instance for HypermediaExternalObjectReference.");
            }

            var routeKeys = this.routeKeyFactory.GetHypermediaRouteKeys(reference);
            return this.GetRouteByType(lookupType, routeKeys);
        }

        public ResolvedRoute ActionToRoute(HypermediaObject actionHostObject, HypermediaActionBase action)
        {
            var lookupType = action.GetType();
            var routeKeys = routeKeyFactory.GetActionRouteKeys(action, actionHostObject);
            return this.GetRouteByType(lookupType, routeKeys);
        }

        public ResolvedRoute TypeToRoute(Type type)
        {
            return this.GetRouteByType(type);
        }

        public bool TryGetRouteByType(Type type, out ResolvedRoute route, object routeKeys = null)
        {
            route = null;
            if (this.RouteRegister.TryGetRoute(type, out var routeInfo))
            {
                route = RouteUrl(routeInfo, routeKeys);
            }
            return route != null;
        }

        public ResolvedRoute RouteUrl(RouteInfo routeInfo, object routeKeys = null)
        {
            var urlString = this.urlHelper.RouteUrl(routeInfo.Name, routeKeys, hypermediaUrlConfig.Scheme, hypermediaUrlConfig.Host.ToUriComponent());

            return new ResolvedRoute(urlString, routeInfo.HttpMethod);
        } 
        
        /// <summary>
        /// Will return a URL for a given route name.
        /// Used HTTP method is unknown here.
        /// </summary>
        /// <param name="routeName"></param>
        /// <param name="routeKeys"></param>
        /// <returns></returns>
        public string RouteUrl(string routeName, object routeKeys = null)
        { 
            return this.urlHelper.RouteUrl(routeName, routeKeys, hypermediaUrlConfig.Scheme, hypermediaUrlConfig.Host.ToUriComponent());
        }

        private ResolvedRoute GetRouteByType(Type lookupType, object routeKeys = null)
        {
            var foundRoute = this.RouteRegister.TryGetRoute(lookupType, out var routeInfo);
            if (!foundRoute)
            {
                return this.HandleUnknownRoute(lookupType);
            }


            var routeUrl = this.urlHelper.RouteUrl(routeInfo.Name, routeKeys, hypermediaUrlConfig.Scheme, hypermediaUrlConfig.Host.ToUriComponent());
            if (routeUrl == null)
            {
                throw new RouteResolverException($"Could not build route: '{routeInfo.Name}' with method {routeInfo.HttpMethod}");
            }

            return new ResolvedRoute(routeUrl, routeInfo.HttpMethod);
        }

        private ResolvedRoute HandleUnknownRoute(Type lookupType)
        {
            if (returnDefaultRouteForUnknownHto)
            {
                return new ResolvedRoute($"{hypermediaUrlConfig.Scheme}://{hypermediaUrlConfig.Host.ToUriComponent()}/{defaultRouteSegmentForUnknownHto}", HttpMethod.Undefined);
            }

            throw new RouteResolverException($"Route to type '{lookupType.Name}' not found in RouteRegister.");
        }
    }
}