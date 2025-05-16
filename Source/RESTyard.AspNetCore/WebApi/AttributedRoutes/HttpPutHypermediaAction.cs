using System;
using Microsoft.AspNetCore.Mvc;

namespace RESTyard.AspNetCore.WebApi.AttributedRoutes
{
    [Obsolete($"Use {nameof(HttpPutAttribute)} in combination with {nameof(HypermediaEndpointAttribute)}")]
    public class HttpPutHypermediaAction : HttpMethodHypermediaAction
    {
        /// <inheritdoc />
        public HttpPutHypermediaAction(Type routeType, Type? routeKeyProducerType = null) : base(
            new[] {Microsoft.AspNetCore.Http.HttpMethods.Put}, routeType, routeKeyProducerType)
        {
        }

        /// <inheritdoc />
        public HttpPutHypermediaAction(string template, Type routeType, Type? routeKeyProducerType = null) : base(
            new[] {Microsoft.AspNetCore.Http.HttpMethods.Put}, template, routeType, routeKeyProducerType)
        {
        }
    }
}