using System;
using System.Reflection;
using RESTyard.AspNetCore.Exceptions;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace RESTyard.AspNetCore.WebApi.AttributedRoutes
{
    public static class AttributedRouteHelper
    {
        public static void EnsureIsRouteKeyProducer(Type? routeKeyProducerType)
        {
            if (routeKeyProducerType != null && !typeof(IKeyProducer).GetTypeInfo().IsAssignableFrom(routeKeyProducerType))
            {
                throw new HypermediaRouteException(
                    $"{nameof(HttpGetHypermediaObject)} requires a {nameof(IKeyProducer)} type.");
            }
        }

        public static void EnsureIs<T>(Type hypermediaObjectType)
        {
            if (!typeof(T).GetTypeInfo().IsAssignableFrom(hypermediaObjectType))
            {
                throw new HypermediaRouteException($"RouteType must be a {typeof(T).Name}");
            }
        }

        public static string EscapeRouteName(string buildName)
        {
            buildName = buildName.Replace('[', '_');
            buildName = buildName.Replace("]", "");
            return buildName;
        }
    }
}