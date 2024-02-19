using System;
using System.Reflection;
using FunicularSwitch;
using Microsoft.AspNetCore.Mvc;
using RESTyard.AspNetCore.Exceptions;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.AspNetCore.Hypermedia.Links;
using RESTyard.AspNetCore.WebApi.ExtensionMethods;

namespace RESTyard.AspNetCore.WebApi.RouteResolver
{
    /// <summary>
    /// Uses a IRouteRegister to access routes by type.
    /// Handles <see cref="ExternalReference"/> for ReferenceToRoute.
    /// </summary>
    public class RegisterRouteResolver : IHypermediaRouteResolver
    {
        protected readonly IRouteRegister RouteRegister;

        private readonly IHypermediaUrlConfig hypermediaUrlConfig;

        private readonly IUrlHelper urlHelper;

        private readonly IRouteKeyFactory routeKeyFactory;
        private readonly bool returnDefaultRouteForUnknownHto;
        private readonly string defaultRouteSegmentForUnknownHto;
        private readonly TypeInfo externalReferenceTypeInfo = typeof(ExternalReference).GetTypeInfo();
        private readonly TypeInfo internalReferenceTypeInfo = typeof(InternalReference).GetTypeInfo();

        public RegisterRouteResolver(IUrlHelper urlHelper, IRouteKeyFactory routeKeyFactory, IRouteRegister routeRegister, HypermediaExtensionsOptions hypermediaOptions, IHypermediaUrlConfig hypermediaUrlConfig)
        {
            RouteRegister = routeRegister;
            this.urlHelper = urlHelper;
            this.hypermediaUrlConfig = hypermediaUrlConfig;
            this.routeKeyFactory = routeKeyFactory;
            returnDefaultRouteForUnknownHto = hypermediaOptions.ReturnDefaultRouteForUnknownHto;
            defaultRouteSegmentForUnknownHto = hypermediaOptions.DefaultRouteSegmentForUnknownHto;
        }

        public ResolvedRoute ObjectToRoute(HypermediaObject hypermediaObject)
        {
            var lookupType = hypermediaObject.GetType();
            var routeKeys = routeKeyFactory.GetHypermediaRouteKeys(hypermediaObject);
            return GetRouteByType(lookupType, routeKeys);
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

                // we assume GET here since external references will be links only for now
                return new ResolvedRoute(externalReferenceObject.ExternalUri.ToString(), HttpMethod.GET, externalReferenceObject.AvailableMediaTypes);
            }     
            
            if (internalReferenceTypeInfo.IsAssignableFrom(lookupType))
            {
                if (reference.GetInstance() is not InternalReference internalReference)
                {
                    throw new HypermediaRouteException("Can not get instance for InternalReference.");
                }

                // we assume get here since external references will be links only for now
                var routeInfo = new RouteInfo(internalReference.RouteName, HttpMethod.GET);
                var resolvedInternalRouteOption = RouteUrl(routeInfo, internalReference.RouteParameters);

                return resolvedInternalRouteOption.Match(
                    resolvedInternalRoute =>
                    {
                        resolvedInternalRoute.AvailableMediaTypes = internalReference.AvailableMediaTypes;
                        return resolvedInternalRoute;
                    },
                    _ => throw new HypermediaRouteException($"Could not build URL to internal route. Name: {internalReference.RouteName}"));
            }

            if (reference is HypermediaExternalObjectReference hypermediaExternalObjectReference)
            {
                // we assume GET here since will it be links or embedded entities only for now
                // for links ExternalReference is simpler and offers MediaTypes
                return new ResolvedRoute(hypermediaExternalObjectReference.Uri.ToString(), HttpMethod.GET);
            }

            var routeKeys = routeKeyFactory.GetHypermediaRouteKeys(reference);
            return GetRouteByType(lookupType, routeKeys);
        }

        public ResolvedRoute ActionToRoute(HypermediaObject actionHostObject, HypermediaActionBase action)
        {
            var lookupType = action.GetType();
            var routeKeys = routeKeyFactory.GetActionRouteKeys(action, actionHostObject);
            return GetRouteByType(lookupType, routeKeys);
        }

        public ResolvedRoute TypeToRoute(Type type)
        {
            return GetRouteByType(type);
        }

        public Option<ResolvedRoute> TryGetRouteByType(Type type, object? routeKeys = null)
        {
            if (RouteRegister.TryGetRoute(type, out var routeInfo))
            {
                return RouteUrl(routeInfo, routeKeys).Match(
                    ok => ok, 
                    _ => Option<ResolvedRoute>.None);
            }
            return Option<ResolvedRoute>.None;
        }

        public Result<ResolvedRoute> RouteUrl(RouteInfo routeInfo, object? routeKeys = null)
        {
            var urlStringResult = urlHelper.RouteUrl(
                routeInfo.Name,
                routeKeys,
                hypermediaUrlConfig.Scheme,
                hypermediaUrlConfig.Host.ToUriComponent())
                ?? Result<string>.Error("Could not build URL");
            return urlStringResult.Map(
                url => new ResolvedRoute(
                    url,
                    routeInfo.HttpMethod,
                    acceptableMediaType: routeInfo.AcceptableMediaType));
        }
        
        /// <summary>
        /// Will return a URL for a given route name.
        /// Used HTTP method is unknown here.
        /// </summary>
        /// <param name="routeName"></param>
        /// <param name="routeKeys"></param>
        /// <returns></returns>
        public Result<string> RouteUrl(string routeName, object? routeKeys = null)
        { 
            return urlHelper.RouteUrl(routeName, routeKeys, hypermediaUrlConfig.Scheme, hypermediaUrlConfig.Host.ToUriComponent()) ?? Result<string>.Error("Could not build URL");
        }

        private ResolvedRoute GetRouteByType(Type lookupType, object? routeKeys = null)
        {
            var foundRoute = RouteRegister.TryGetRoute(lookupType, out var routeInfo);
            if (!foundRoute)
            {
                return HandleUnknownRoute(lookupType);
            }

            var routeUrl = urlHelper.RouteUrl(routeInfo.Name, routeKeys, hypermediaUrlConfig.Scheme, hypermediaUrlConfig.Host.ToUriComponent());
            if (routeUrl == null)
            {
                throw new RouteResolverException($"Could not build route: '{routeInfo.Name}' with method {routeInfo.HttpMethod}");
            }

            return new ResolvedRoute(routeUrl, routeInfo.HttpMethod, acceptableMediaType: routeInfo.AcceptableMediaType);
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