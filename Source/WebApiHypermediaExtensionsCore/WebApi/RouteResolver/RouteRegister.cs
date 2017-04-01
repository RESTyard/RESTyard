using System;
using System.Reflection;
using System.Collections.Generic;
using WebApiHypermediaExtensionsCore.Exceptions;
using WebApiHypermediaExtensionsCore.Hypermedia;
using WebApiHypermediaExtensionsCore.Hypermedia.Actions;

namespace WebApiHypermediaExtensionsCore.WebApi.RouteResolver
{
    public class RouteRegister : IRouteRegister
    {
        private readonly Dictionary<Type, string> routeRegister;

        private readonly Dictionary<Type, IKeyProducer> routeKeyProducerRegister;

        public RouteRegister()
        {
            routeRegister  = new Dictionary<Type, string>();
            routeKeyProducerRegister = new Dictionary<Type, IKeyProducer>();
        }

        public string GetRoute(Type lookupType)
        {
            string routeName;
            if (!this.routeRegister.TryGetValue(lookupType, out routeName))
            {
                throw new RouteRegisterException($"Route to type '{lookupType}' not found in RouteRegister.");
            }

            return routeName;
        }

        public void AddActionRoute(Type hypermediaActionType, string routeName)
        {
            if (!IsHypermediaAction(hypermediaActionType) /*&& !IsGenericHypermediaAction(hypermediaActionType)*/)
            {
                throw new RouteRegisterException(
                    $"Type {hypermediaActionType} must derive from {typeof(HypermediaAction<>).Name}.");
            }

            this.AddRoute(hypermediaActionType, routeName);
        }

        private static bool IsHypermediaAction(Type hypermediaActionType)
        {
            return typeof(HypermediaActionBase).IsAssignableFrom(hypermediaActionType);
        }

        public void AddHypermediaObjectRoute(Type hypermediaObjectType, string routeName)
        {
            if (!typeof(HypermediaObject).IsAssignableFrom(hypermediaObjectType))
            {
                throw new RouteRegisterException(
                    $"Type {hypermediaObjectType} must derive from {typeof(HypermediaObject).Name}.");
            }

            this.AddRoute(hypermediaObjectType, routeName);
        }

        public void AddParameterTypeRoute(Type iHypermediaActionParameter, string routeName)
        {
            if (!typeof(IHypermediaActionParameter).IsAssignableFrom(iHypermediaActionParameter))
            {
                throw new RouteRegisterException(
                    $"Type {iHypermediaActionParameter} must derive from {typeof(IHypermediaActionParameter).Name}.");
            }

            this.AddRoute(iHypermediaActionParameter, routeName);
        }

        public void AddRouteKeyProducer(Type keySourceType, IKeyProducer keyProducer)
        {
            if (this.RouteKeyProducerExists(keySourceType))
            {
                throw new RouteRegisterException($"RouteKeyProducer for {keySourceType} already exists.");
            }

            this.routeKeyProducerRegister.Add(keySourceType, keyProducer);
        }

        public IKeyProducer GetKeyProducer(Type type)
        {
            IKeyProducer keyProducer;
            this.routeKeyProducerRegister.TryGetValue(type, out keyProducer);

            return keyProducer;
        }

        private void AddRoute(Type type, string routeName)
        {
            if (this.RouteExists(type))
            {
                throw new RouteRegisterException($"Route to type {type} already exists.");
            }

            this.routeRegister.Add(type, routeName);
        }

        private bool RouteExists(Type lookupType)
        {
            string routeName;
            if (this.routeRegister.TryGetValue(lookupType, out routeName))
            {
                return true;
            }

            return false;
        }

        private bool RouteKeyProducerExists(Type lookupType)
        {
            IKeyProducer producer;
            if (this.routeKeyProducerRegister.TryGetValue(lookupType, out producer))
            {
                return true;
            }

            return false;
        }
    }
}
