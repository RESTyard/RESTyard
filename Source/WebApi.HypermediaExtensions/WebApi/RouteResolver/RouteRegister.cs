using System;
using System.Reflection;
using System.Collections.Generic;
using WebApi.HypermediaExtensions.Exceptions;
using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.Hypermedia.Actions;

namespace WebApi.HypermediaExtensions.WebApi.RouteResolver
{
    public class RouteRegister : IRouteRegister
    {
        private readonly Dictionary<Type, RouteInfo> routeRegister;

        private readonly Dictionary<Type, IKeyProducer> routeKeyProducerRegister;

        public RouteRegister()
        {
            routeRegister  = new Dictionary<Type, RouteInfo>();
            routeKeyProducerRegister = new Dictionary<Type, IKeyProducer>();
        }

        public bool TryGetRoute(Type lookupType, out RouteInfo routeInfo)
        {
            if (!this.routeRegister.TryGetValue(lookupType, out routeInfo))
            {
                routeInfo = RouteInfo.Empty();
                return false;
               
            }

            return true;
        }

        public void AddActionRoute(Type hypermediaActionType, string routeName, HttpMethod httpMethod, string acceptableMediaType = null)
        {
            if (!IsHypermediaAction(hypermediaActionType) /*&& !IsGenericHypermediaAction(hypermediaActionType)*/)
            {
                throw new RouteRegisterException(
                    $"Type {hypermediaActionType} must derive from {typeof(HypermediaAction<>).Name}.");
            }

            this.AddRoute(hypermediaActionType, new RouteInfo(routeName, httpMethod, acceptableMediaType));
        }

        private static bool IsHypermediaAction(Type hypermediaActionType)
        {
            return typeof(HypermediaActionBase).GetTypeInfo().IsAssignableFrom(hypermediaActionType);
        }

        public void AddHypermediaObjectRoute(Type hypermediaObjectType, string routeName, HttpMethod httpMethod)
        {
            if (!typeof(HypermediaObject).GetTypeInfo().IsAssignableFrom(hypermediaObjectType))
            {
                throw new RouteRegisterException(
                    $"Type {hypermediaObjectType} must derive from {typeof(HypermediaObject).Name}.");
            }

            this.AddRoute(hypermediaObjectType, new RouteInfo(routeName, httpMethod));
        }

        public void AddParameterTypeRoute(Type iHypermediaActionParameter, string routeName, HttpMethod httpMethod)
        {
            if (!typeof(IHypermediaActionParameter).GetTypeInfo().IsAssignableFrom(iHypermediaActionParameter))
            {
                throw new RouteRegisterException(
                    $"Type {iHypermediaActionParameter} must derive from {typeof(IHypermediaActionParameter).Name}.");
            }

            this.AddRoute(iHypermediaActionParameter, new RouteInfo(routeName, httpMethod));
        }

        public void AddRouteKeyProducer(Type keySourceType, IKeyProducer keyProducer)
        {
            if (this.RouteKeyProducerExists(keySourceType))
            {
                throw new RouteRegisterException($"RouteKeyProducer for {keySourceType} already exists.");
            }

            this.routeKeyProducerRegister.Add(keySourceType, keyProducer);
        }

        public bool TryGetKeyProducer(Type type, out IKeyProducer keyProducer)
        {
            return this.routeKeyProducerRegister.TryGetValue(type, out keyProducer);
        }

        private void AddRoute(Type type, RouteInfo routeInfo)
        {
            if (this.RouteExists(type))
            {
                throw new RouteRegisterException($"Route to type {type} already exists.");
            }

            this.routeRegister.Add(type, routeInfo);
        }

        private bool RouteExists(Type lookupType)
        {
            if (this.routeRegister.TryGetValue(lookupType, out _))
            {
                return true;
            }

            return false;
        }

        private bool RouteKeyProducerExists(Type lookupType)
        {
            if (this.routeKeyProducerRegister.TryGetValue(lookupType, out _))
            {
                return true;
            }

            return false;
        }
    }
}
