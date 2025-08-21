using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Logging;
using RESTyard.AspNetCore.Exceptions;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.WebApi.AttributedRoutes;

namespace RESTyard.AspNetCore.WebApi.RouteResolver
{
    public class AttributedRoutesRegister : RouteRegister
    {
        private readonly ILogger logger;

        public AttributedRoutesRegister(IHypermediaApiExplorer apiExplorer, ILogger<AttributedRoutesRegister> logger)
        {
            this.logger = logger;
            RegisterApiRoutes(apiExplorer);
        }

        private void RegisterApiRoutes(IHypermediaApiExplorer apiExplorer)
        {
            var apiDescriptions = apiExplorer.GetHypermediaEndpoints();
            foreach (var (apiDescription, htoEndpoint) in WithMetadata<IHypermediaObjectEndpointMetadata>(
                         apiDescriptions))
            {
                if (!HttpMethods.IsGet(apiDescription.HttpMethod ?? ""))
                {
                    throw new ArgumentOutOfRangeException(nameof(apiDescription.HttpMethod),
                        apiDescription.HttpMethod, apiDescription.HttpMethod);
                }
                this.AddHypermediaObjectRoute(htoEndpoint.RouteType, htoEndpoint.EndpointName, HttpMethods.Get);
                this.AddRouteKeyProducer(apiDescription, htoEndpoint);
            }

            foreach (var (apiDescription, actionEndpoint) in WithMetadata<IHypermediaActionEndpointMetadata>(
                         apiDescriptions))
            {
                var httpMethod = apiDescription.HttpMethod;
                var isValid = httpMethod is not null
                              && (HttpMethods.IsPost(httpMethod)
                                  || HttpMethods.IsPut(httpMethod)
                                  || HttpMethods.IsPatch(httpMethod)
                                  || HttpMethods.IsDelete(httpMethod));
                if (!isValid)
                {
                    throw new ArgumentOutOfRangeException(nameof(apiDescription.HttpMethod),
                        apiDescription.HttpMethod, apiDescription.HttpMethod);
                }
                this.AddActionRoute(actionEndpoint.ActionType, actionEndpoint.EndpointName, httpMethod!, actionEndpoint.AcceptedMediaType);
                this.TryAddDefaultRouteKeyProducer(apiDescription, actionEndpoint.RouteType);
            }

            foreach (var (apiDescription, actionParameterInfoEndpoint) in
                     WithMetadata<IHypermediaActionParameterInfoEndpointMetadata>(apiDescriptions))
            {
                if (!HttpMethods.IsGet(apiDescription.HttpMethod ?? ""))
                {
                    throw new HypermediaException(
                        $"Unsupported HTTP verb {apiDescription.HttpMethod} on parameter info endpoint for type {actionParameterInfoEndpoint.RouteType}");
                }

                this.AddParameterTypeRoute(actionParameterInfoEndpoint.RouteType,
                    actionParameterInfoEndpoint.EndpointName, HttpMethods.Get);
            }
            return;

            static IEnumerable<(ApiDescription ApiDescription, TMetadata Metadata)> WithMetadata<TMetadata>(IEnumerable<ApiDescription> items)
            {
                foreach (var apiDescription in items)
                {
                    var metadata = apiDescription.ActionDescriptor.EndpointMetadata.OfType<TMetadata>()
                        .FirstOrDefault();
                    if (metadata is not null)
                    {
                        yield return (apiDescription, metadata);
                    }
                }
            }
        }

        private void AddRouteKeyProducer(ApiDescription apiDescription, IHypermediaObjectEndpointMetadata hmoEndpoint)
        {
            if (hmoEndpoint.RouteKeyProducerType != null)
            {
                if (typeof(HypermediaQueryResult).GetTypeInfo().IsAssignableFrom(hmoEndpoint.RouteType))
                {
                    throw new RouteRegisterException(
                        $"Routes to Query's may not require a key '{hmoEndpoint.RouteType}'. Queries should not be handled on a Entity.");
                }

                AttributedRouteHelper.AssertIsRouteKeyProducer(hmoEndpoint.RouteKeyProducerType);
                var keyProducer = (IKeyProducer)Activator.CreateInstance(hmoEndpoint.RouteKeyProducerType)!;
                this.AddRouteKeyProducer(hmoEndpoint.RouteType, keyProducer);
            }
            else
            {
                TryAddDefaultRouteKeyProducer(apiDescription, hmoEndpoint.RouteType);
            }
        }

        private void TryAddDefaultRouteKeyProducer(ApiDescription apiDescription, Type routeType)
        {
            if (this.TryGetKeyProducer(routeType, out _))
            {
                return;
            }

            var templateToUse = apiDescription.RelativePath ?? string.Empty;

            var template = TemplateParser.Parse(templateToUse);
            if (template.Parameters.Count > 0)
            {
                this.AddRouteKeyProducer(
                    routeType,
                    RouteKeyProducer.Create(routeType, template.Parameters.Select(p => p.Name).ToList()));
            }
        }
    }
}