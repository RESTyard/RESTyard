using System;

namespace RESTyard.AspNetCore.WebApi.AttributedRoutes
{
    public class HttpDeleteHypermediaAction : HttpMethodHypermediaAction
    {
        /// <inheritdoc />
        public HttpDeleteHypermediaAction(Type routeType, Type? routeKeyProducerType = null) : base(
            new[] {Microsoft.AspNetCore.Http.HttpMethods.Delete}, routeType, routeKeyProducerType)
        {
        }

        /// <inheritdoc />
        public HttpDeleteHypermediaAction(string template, Type routeType, Type? routeKeyProducerType = null) : base(
            new[] {Microsoft.AspNetCore.Http.HttpMethods.Delete}, template, routeType, routeKeyProducerType)
        {
        }
    }
}