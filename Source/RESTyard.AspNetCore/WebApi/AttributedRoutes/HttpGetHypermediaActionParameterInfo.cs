using System;
using Microsoft.AspNetCore.Mvc;
using RESTyard.AspNetCore.Hypermedia.Actions;

namespace RESTyard.AspNetCore.WebApi.AttributedRoutes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class HttpGetHypermediaActionParameterInfo : HttpGetAttribute, IHaveRouteInfo
    {
        public Type RouteType { get; private set; }
        public Type? RouteKeyProducerType { get; } = null;
        public string? AcceptedMediaType => null;

        /// <summary>
        /// Indicates a route to a Type which is aused in an action. The route should provide type information.
        /// </summary>
        /// <param name="routeType">the parameter type which will be described by the response.</param>
        public HttpGetHypermediaActionParameterInfo(Type routeType)
        {
            (RouteType, Name) = Init(routeType);
        }

        /// <summary>
        /// Indicates a route to a Type which is aused in an action. The route should provide type information.
        /// </summary>
        /// <param name="template">The route template</param>
        /// <param name="routeType">the parameter type which will be described by the response.</param>
        public HttpGetHypermediaActionParameterInfo(string template, Type routeType) : base(template)
        {
            (RouteType, Name) = Init(routeType);
        }

        private static (Type RouteType, string Name) Init(Type routeType)
        {
            AttributedRouteHelper.EnsureIs<IHypermediaActionParameter>(routeType);
            return (routeType, AttributedRouteHelper.EscapeRouteName("GenericRouteName_ActionParameterInfo_" + routeType));
        }
    }
}