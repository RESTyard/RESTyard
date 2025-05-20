using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Template;
using RESTyard.AspNetCore.Exceptions;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Attributes;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace RESTyard.AspNetCore.WebApi.AttributedRoutes
{
    [Obsolete($"Use {nameof(HttpGetAttribute)} in combination with {nameof(HypermediaObjectEndpointAttribute<IHypermediaObject>)}")]
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class HttpGetHypermediaObject : HttpGetAttribute, IHaveRouteInfo
    {
        public Type RouteType { get; private set; }

        public Type? RouteKeyProducerType { get; private set; }
        
        public string? AcceptedMediaType => null;

        /// <summary>
        /// Indicates that this Route will provide a <see cref="IHypermediaObject"/>. It is not required that only this <see cref="IHypermediaObject"/> can be returned.
        /// This route will be referenced if it's type is used in a <see cref="IHypermediaObject"/>.
        /// </summary>
        /// <param name="routeType">The type of the <see cref="IHypermediaObject"/> assiciated with this route. No other Route may have the same type.</param>
        /// <param name="routeKeyProducerType">If the route template contains a (single) key it is required that the type of teh responsible RouteKeyProducer is given.
        /// This type will be used to create an instance of the producer and generate the key object used in a UrlHelper to determine the final URL.
        /// </param>
        public HttpGetHypermediaObject(Type routeType, Type? routeKeyProducerType = null)
        {
            (Name, RouteType, RouteKeyProducerType) = Init(routeType, routeKeyProducerType);
        }

        /// <summary>
        /// Indicates that this Route will provide an <see cref="IHypermediaObject"/>. It is not required that only this <see cref="IHypermediaObject"/> can be returned.
        /// This route will be referenced if it's type is used in an <see cref="IHypermediaObject"/>.
        /// </summary>
        /// <param name="template">The route template.</param>
        /// <param name="routeType">The type of the <see cref="IHypermediaObject"/> associated with this route. No other Route may have the same type.</param>
        /// <param name="routeKeyProducerType">If the route template contains a (single) key it is required that the type of teh responsible RouteKeyProducer is given.
        /// This type will be used to create a n instance of the producer and generate the key object used in a UrlHelper to determine the final URL.
        /// </param>
        public HttpGetHypermediaObject(string template, Type routeType, Type? routeKeyProducerType = null) : base (template)
        {
            (Name, RouteType, RouteKeyProducerType) = Init(routeType, routeKeyProducerType);

            var routeTemplate = TemplateParser.Parse(template);
            if (routeTemplate.Parameters.Count > 0 && routeKeyProducerType == null 
                                                   && routeType.GetTypeInfo().GetProperties().All(p => p.GetCustomAttribute<KeyAttribute>() == null))
            {
                throw new HypermediaRouteException($"Route '{this.Name}' with parameters requires either a RouteKeyProducer type or properties with attribute KeyAttribute on type {routeType.Name}.");
            }
        }

        private static (string Name, Type RouteType, Type? RouteKeyProducerType) Init(Type routeType, Type? routeKeyProducerType)
        {
            AttributedRouteHelper.EnsureHas<HypermediaObjectAttribute>(routeType);
            AttributedRouteHelper.EnsureIsRouteKeyProducer(routeKeyProducerType);
            var name = AttributedRouteHelper.EscapeRouteName("GenericRouteName_HypermediaObject_" + routeType);
            return (name, routeType, routeKeyProducerType);
        }
    }
}