using System;
using Microsoft.AspNetCore.Mvc;
using RESTyard.AspNetCore.Hypermedia;

namespace RESTyard.AspNetCore.WebApi.AttributedRoutes
{
    [Obsolete($"Use {nameof(HttpPatchAttribute)} in combination with {nameof(HypermediaActionEndpointAttribute<IHypermediaObject>)}")]
    public class HttpPatchHypermediaAction : HttpMethodHypermediaAction
    {
        /// <inheritdoc />
        public HttpPatchHypermediaAction(Type routeType, Type? routeKeyProducerType = null) : base(
            new[] {Microsoft.AspNetCore.Http.HttpMethods.Patch}, routeType, routeKeyProducerType)
        {
        }

        /// <inheritdoc />
        public HttpPatchHypermediaAction(string template, Type routeType, Type? routeKeyProducerType = null) : base(
            new[] {Microsoft.AspNetCore.Http.HttpMethods.Patch}, template, routeType, routeKeyProducerType)
        {
        }
    }
}