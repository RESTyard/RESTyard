using System;
using System.Diagnostics;
using System.Reflection;
using RESTyard.AspNetCore.Exceptions;
using RESTyard.AspNetCore.Util;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace RESTyard.AspNetCore.WebApi.AttributedRoutes
{
    public static class AttributedRouteHelper
    {
        private static bool IsRouteKeyProducer(Type? routeKeyProducerType) =>
            routeKeyProducerType == null ||
            typeof(IKeyProducer).GetTypeInfo().IsAssignableFrom(routeKeyProducerType);

        public static void EnsureIsRouteKeyProducer(Type? routeKeyProducerType)
        {
            if (!IsRouteKeyProducer(routeKeyProducerType))
            {
                throw new HypermediaRouteException(
                    $"{routeKeyProducerType?.BeautifulName() ?? "NULL"} must implement {nameof(IKeyProducer)}.");
            }
        }

        public static void AssertIsRouteKeyProducer(Type? routeKeyProducerType)
        {
            Debug.Assert(IsRouteKeyProducer(routeKeyProducerType));
        }

        public static bool Is<T>(Type hypermediaObjectType)
        {
            return typeof(T).GetTypeInfo().IsAssignableFrom(hypermediaObjectType);
        }

        public static bool Has<T>(Type hypermediaObjectType) where T : Attribute
        {
            return hypermediaObjectType.GetCustomAttribute<T>() != null;
        }

        public static void EnsureIs<T>(Type hypermediaObjectType)
        {
            if (!Is<T>(hypermediaObjectType))
            {
                throw new HypermediaRouteException($"RouteType must be a {typeof(T).Name}");
            }
        }

        public static void EnsureHas<T>(Type hypermediaObjectType) where T : Attribute
        {
            if (!Has<T>(hypermediaObjectType))
            {
                throw new HypermediaRouteException($"RouteType must have attribute {typeof(T).Name}");
            }
        }

        public static void AssertIs<T>(Type hypermediaObjectType)
        {
            Debug.Assert(Is<T>(hypermediaObjectType));
        }

        public static string EscapeRouteName(string buildName)
        {
            buildName = buildName.Replace('[', '_');
            buildName = buildName.Replace("]", "");
            return buildName;
        }
    }
}