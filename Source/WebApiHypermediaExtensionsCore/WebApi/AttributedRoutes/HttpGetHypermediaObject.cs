using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Template;
using WebApiHypermediaExtensionsCore.Exceptions;
using WebApiHypermediaExtensionsCore.Hypermedia;

namespace WebApiHypermediaExtensionsCore.WebApi.AttributedRoutes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class HttpGetHypermediaObject : HttpGetAttribute, IHaveRouteInfo
    {
        public Type RouteType { get; private set; }

        public Type RouteKeyProducerType { get; private set; }

        /// <summary>
        /// Indicates that this Rout will provide a <see cref="HypermediaObject"/>. It is not required that only this <see cref="HypermediaObject"/> can be returned.
        /// This route will be referenced if it's type is used in a <see cref="HypermediaObject"/>.
        /// </summary>
        /// <param name="routeType">The type of the <see cref="HypermediaObject"/> assiciated with this route. No other Route may have the same type.</param>
        /// <param name="routeKeyProducerType">If the route template contains a (single) key it is required that the type of teh responsible RouteKeyProducer is given.
        /// This type will be used to create a n instance of the producer and generate the key object used in a UrlHelper to determin the final URL.
        /// </param>
        public HttpGetHypermediaObject(Type routeType, Type routeKeyProducerType = null)
        {
            Init(routeType, routeKeyProducerType);
        }

        /// <summary>
        /// Indicates that this Rout will provide a <see cref="HypermediaObject"/>. It is not required that only this <see cref="HypermediaObject"/> can be returned.
        /// This route will be referenced if it's type is used in a <see cref="HypermediaObject"/>.
        /// </summary>
        /// <param name="template">The route template.</param>
        /// <param name="routeType">The type of the <see cref="HypermediaObject"/> assiciated with this route. No other Route may have the same type.</param>
        /// <param name="routeKeyProducerType">If the route template contains a (single) key it is required that the type of teh responsible RouteKeyProducer is given.
        /// This type will be used to create a n instance of the producer and generate the key object used in a UrlHelper to determin the final URL.
        /// </param>y>


        public HttpGetHypermediaObject(string template, Type routeType, Type routeKeyProducerType = null) : base (template)
        {
            Init(routeType, routeKeyProducerType);

            var routeTemplate = TemplateParser.Parse(template);
            if (routeTemplate.Parameters.Count > 0 && routeKeyProducerType == null)
            {
                throw new HypermediaRouteException($"Route '{this.Name}' with parameters require a RouteKeyProducer Type.");
            }
        }

        private void Init(Type routeType, Type routeKeyProducerType)
        {
            AttributedRouteHelper.EnsureIs<HypermediaObject>(routeType);
            AttributedRouteHelper.EnsureIsRouteKeyProducer(routeKeyProducerType);
            Name = AttributedRouteHelper.EscapeRouteName("GenericRouteName_HypermediaObject_" + routeType);
            RouteType = routeType;
            RouteKeyProducerType = routeKeyProducerType;
        }
    }
}