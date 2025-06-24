using System;
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
            foreach (var apiDescription in apiExplorer.GetHypermediaEndpoints())
            {
                var htoEndpoint = apiDescription.ActionDescriptor.EndpointMetadata
                    .OfType<IHypermediaObjectEndpointMetadata>().FirstOrDefault();
                if (htoEndpoint is not null)
                {
                    if (!HttpMethods.IsGet(apiDescription.HttpMethod ?? ""))
                    {
                        throw new ArgumentOutOfRangeException(nameof(apiDescription.HttpMethod),
                            apiDescription.HttpMethod, apiDescription.HttpMethod);
                    }
                    this.AddHypermediaObjectRoute(htoEndpoint.RouteType, htoEndpoint.EndpointName, HttpMethods.Get);
                    this.AddRouteKeyProducer(apiDescription, htoEndpoint);
                }

                var actionEndpoint = apiDescription.ActionDescriptor.EndpointMetadata
                    .OfType<IHypermediaActionEndpointMetadata>().FirstOrDefault();
                if (actionEndpoint is not null)
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
                }

                var actionParameterInfoEndpoint = apiDescription.ActionDescriptor.EndpointMetadata
                    .OfType<IHypermediaActionParameterInfoEndpointMetadata>().FirstOrDefault();
                if (actionParameterInfoEndpoint is not null)
                {
                    if (!HttpMethods.IsGet(apiDescription.HttpMethod ?? ""))
                    {
                        throw new HypermediaException(
                            $"Unsupported HTTP verb {apiDescription.HttpMethod} on parameter info endpoint for type {actionParameterInfoEndpoint.RouteType}");
                    }

                    this.AddParameterTypeRoute(actionParameterInfoEndpoint.RouteType,
                        actionParameterInfoEndpoint.EndpointName, HttpMethods.Get);
                }
            }
        }

        private void AddRouteKeyProducer(ApiDescription apiDescription, IHypermediaObjectEndpointMetadata hmoEndpoint)
        {
            var autoAddRouteKeyProducers = HttpMethods.IsGet(apiDescription.HttpMethod ?? "");
          
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
            else if (autoAddRouteKeyProducers && !typeof(HypermediaQueryResult).GetTypeInfo().IsAssignableFrom(hmoEndpoint.RouteType))
            {
                var templateToUse = apiDescription.RelativePath ?? string.Empty;

                var template = TemplateParser.Parse(templateToUse);
                if (template.Parameters.Count > 0)
                {
                    this.AddRouteKeyProducer(
                        hmoEndpoint.RouteType,
                        RouteKeyProducer.Create(hmoEndpoint.RouteType, template.Parameters.Select(p => p.Name).ToList()));
                }
            }
        }
    }
}