using System;
using System.Reflection;
using RESTyard.WebApi.Extensions.Exceptions;
using RESTyard.WebApi.Extensions.WebApi.RouteResolver;

namespace RESTyard.WebApi.Extensions.WebApi.AttributedRoutes
{
    public static class AttributedRouteHelper
    {
        public static void EnsureIsRouteKeyProducer(Type routeKeyProducerType)
        {
            if (routeKeyProducerType != null && !typeof(IKeyProducer).GetTypeInfo().IsAssignableFrom(routeKeyProducerType))
            {
                throw new HypermediaRouteException(
                    $"{typeof(HttpGetHypermediaObject).Name} requires a {typeof(IKeyProducer).Name} type.");
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