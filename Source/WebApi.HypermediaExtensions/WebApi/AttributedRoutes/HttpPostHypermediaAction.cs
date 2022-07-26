using System;

namespace RESTyard.WebApi.Extensions.WebApi.AttributedRoutes
{
    public class HttpPostHypermediaAction : HttpMethodHypermediaAction
    {
        /// <inheritdoc />
        public HttpPostHypermediaAction(Type routeType, Type routeKeyProducerType = null) : base(
            new[] {Microsoft.AspNetCore.Http.HttpMethods.Post}, routeType, routeKeyProducerType)
        {
        }

        /// <inheritdoc />
        public HttpPostHypermediaAction(string template, Type routeType, Type routeKeyProducerType = null) : base(
            new[] {Microsoft.AspNetCore.Http.HttpMethods.Post}, template, routeType, routeKeyProducerType)
        {
        }
    }
}