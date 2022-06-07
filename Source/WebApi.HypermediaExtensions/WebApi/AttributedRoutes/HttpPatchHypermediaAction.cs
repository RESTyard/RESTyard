using System;

namespace WebApi.HypermediaExtensions.WebApi.AttributedRoutes
{
    public class HttpPatchHypermediaAction : HttpMethodHypermediaAction
    {
        /// <inheritdoc />
        public HttpPatchHypermediaAction(Type routeType, Type routeKeyProducerType = null) : base(
            new[] {Microsoft.AspNetCore.Http.HttpMethods.Patch}, routeType, routeKeyProducerType)
        {
        }

        /// <inheritdoc />
        public HttpPatchHypermediaAction(string template, Type routeType, Type routeKeyProducerType = null) : base(
            new[] {Microsoft.AspNetCore.Http.HttpMethods.Patch}, template, routeType, routeKeyProducerType)
        {
        }
    }
}