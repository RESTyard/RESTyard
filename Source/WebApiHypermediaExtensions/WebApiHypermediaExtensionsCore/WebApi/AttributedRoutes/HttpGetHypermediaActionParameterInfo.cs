using System;
using Microsoft.AspNetCore.Mvc;
using WebApiHypermediaExtensionsCore.Hypermedia.Actions;

namespace WebApiHypermediaExtensionsCore.WebApi.AttributedRoutes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class HttpGetHypermediaActionParameterInfo : HttpGetAttribute, IHaveRouteInfo
    {
        public Type RouteType { get; private set; }
        public Type RouteKeyProducerType { get; } = null;
        /// <summary>
        /// Indicates a route to a Type which is aused in an action. The route should provide type information.
        /// </summary>
        /// <param name="routeType">the parameter type which will be described by the response.</param>
        public HttpGetHypermediaActionParameterInfo(Type routeType)
        {
            Init(routeType);
        }

        /// <summary>
        /// Indicates a route to a Type which is aused in an action. The route should provide type information.
        /// </summary>
        /// <param name="template">The route template</param>
        /// <param name="routeType">the parameter type which will be described by the response.</param>
        public HttpGetHypermediaActionParameterInfo(string template, Type routeType) : base(template)
        {
            Init(routeType);
        }

        private void Init(Type routeType)
        {
            AttributedRouteHelper.EnsureIs<IHypermediaActionParameter>(routeType);
            Name = AttributedRouteHelper.EscapeRouteName("GenericRouteName_ActionParameterInfo_" + routeType);
            RouteType = routeType;
        }
    }
}