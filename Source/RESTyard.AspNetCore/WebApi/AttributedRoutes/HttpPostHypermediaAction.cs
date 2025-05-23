using System;
using Microsoft.AspNetCore.Mvc;
using RESTyard.AspNetCore.Hypermedia;

namespace RESTyard.AspNetCore.WebApi.AttributedRoutes
{
    [Obsolete($"Use {nameof(HttpPostAttribute)} in combination with {nameof(HypermediaActionEndpointAttribute<IHypermediaObject>)}")]
    public class HttpPostHypermediaAction : HttpMethodHypermediaAction
    {
        /// <inheritdoc />
        public HttpPostHypermediaAction(Type routeType, Type? routeKeyProducerType = null) : base(
            new[] {Microsoft.AspNetCore.Http.HttpMethods.Post}, routeType, routeKeyProducerType)
        {
        }

        /// <inheritdoc />
        public HttpPostHypermediaAction(string template, Type routeType, Type? routeKeyProducerType = null) : base(
            new[] {Microsoft.AspNetCore.Http.HttpMethods.Post}, template, routeType, routeKeyProducerType)
        {
        }
    }
}