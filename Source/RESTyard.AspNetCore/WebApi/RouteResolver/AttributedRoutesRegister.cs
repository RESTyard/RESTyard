using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Logging;
using RESTyard.AspNetCore.Exceptions;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Util;
using RESTyard.AspNetCore.Util.Extensions;
using RESTyard.AspNetCore.WebApi.AttributedRoutes;
using RESTyard.AspNetCore.WebApi.ExtensionMethods;

namespace RESTyard.AspNetCore.WebApi.RouteResolver
{
    public class AttributedRoutesRegister : RouteRegister
    {
        private readonly ILogger logger;

        public AttributedRoutesRegister(HypermediaExtensionsOptions hypermediaOptions, ILogger<AttributedRoutesRegister> logger)
        {
            this.logger = logger;
            var assembliesToCrawl = hypermediaOptions.ControllerAndHypermediaAssemblies.Length > 0
                ? hypermediaOptions.ControllerAndHypermediaAssemblies
                : Assembly.GetEntryAssembly()?.Yield() ?? [];

            foreach (var assemblyToCrawl in assembliesToCrawl)
            {
                RegisterAssemblyRoutes(assemblyToCrawl);
            }
        }

        private void RegisterAssemblyRoutes(Assembly assembly)
        {
            var assemblyMethods = assembly.GetTypes().SelectMany(t => t.GetTypeInfo().GetMethods());
            foreach (var method in assemblyMethods)
            {
                if (TryGetHypermediaEndpoint<HypermediaEndpointAttribute>(method, out var endpointAttribute, out var httpAttribute))
                {
                    AssertAttributeHasName(endpointAttribute);

                    switch (endpointAttribute)
                    {
                        case IHypermediaObjectEndpointMetadata htoEndpoint:
                            switch (httpAttribute)
                            {
                                case HttpGetAttribute:
                                    this.AddHypermediaObjectRoute(htoEndpoint.RouteType, htoEndpoint.EndpointName, HttpMethod.GET);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(nameof(httpAttribute),
                                        $"Invalid http attribute: {httpAttribute.GetType().Name} on {nameof(HypermediaObjectEndpointAttribute<IHypermediaObject>)}");
                            }
                            this.AddRouteKeyProducer(method, htoEndpoint, httpAttribute);

                            break;
                        case IHypermediaActionEndpointMetadata actionEndpoint:
                            this.AddActionRoute(
                                actionEndpoint.ActionType,
                                actionEndpoint.EndpointName,
                                httpAttribute switch
                                {
                                    HttpPostAttribute => HttpMethod.POST,
                                    HttpPutAttribute => HttpMethod.PUT,
                                    HttpPatchAttribute => HttpMethod.PATCH,
                                    HttpDeleteAttribute => HttpMethod.DELETE,
                                    _ => throw new ArgumentOutOfRangeException(nameof(httpAttribute),
                                        $"Invalid http attribute: {httpAttribute.GetType().Name} on {nameof(HypermediaActionEndpointAttribute<IHypermediaObject>)}"),
                                },
                                actionEndpoint.AcceptedMediaType);
                            break;
                        case IHypermediaActionParameterInfoEndpointMetadata actionParameterInfoMetadata:
                            switch (httpAttribute)
                            {
                                case HttpGetAttribute:
                                    this.AddParameterTypeRoute(actionParameterInfoMetadata.RouteType, actionParameterInfoMetadata.EndpointName, HttpMethod.GET);
                                    break;
                                default:
                                    throw new HypermediaException(
                                        $"Unsupported HTTP verb {httpAttribute.GetType().BeautifulName()} on parameter info endpoint");
                            }

                            break;
                    }

                }
                else if (TryGetSingleHypermediaAttribute(method, out var hypermediaAttribute))
                {
                    AssertAttributeHasName(hypermediaAttribute);

                    switch (hypermediaAttribute)
                    {
                        case HttpGetHypermediaObject httpGetHypermediaObject:
                            this.AddHypermediaObjectRoute(httpGetHypermediaObject.RouteType,
                                httpGetHypermediaObject.Name!, HttpMethod.GET);
                            AddRouteKeyProducer(method, httpGetHypermediaObject);
                            break;
                        case HttpPostHypermediaAction httpPostHypermediaAction:
                            this.AddActionRoute(httpPostHypermediaAction.RouteType, httpPostHypermediaAction.Name!,
                                HttpMethod.POST, httpPostHypermediaAction.AcceptedMediaType);
                            AddRouteKeyProducer(method, httpPostHypermediaAction);
                            break;
                        case HttpPutHypermediaAction httpPutHypermediaAction:
                            this.AddActionRoute(httpPutHypermediaAction.RouteType, httpPutHypermediaAction.Name!,
                                HttpMethod.PUT, httpPutHypermediaAction.AcceptedMediaType);
                            AddRouteKeyProducer(method, httpPutHypermediaAction);
                            break;
                        case HttpDeleteHypermediaAction httpDeleteHypermediaAction:
                            this.AddActionRoute(httpDeleteHypermediaAction.RouteType, httpDeleteHypermediaAction.Name!,
                                HttpMethod.DELETE, httpDeleteHypermediaAction.AcceptedMediaType);
                            AddRouteKeyProducer(method, httpDeleteHypermediaAction);
                            break;
                        case HttpPatchHypermediaAction httpPatchHypermediaAction:
                            this.AddActionRoute(httpPatchHypermediaAction.RouteType, httpPatchHypermediaAction.Name!,
                                HttpMethod.PATCH, httpPatchHypermediaAction.AcceptedMediaType);
                            AddRouteKeyProducer(method, httpPatchHypermediaAction);
                            break;
                        case HttpGetHypermediaActionParameterInfo httpGetHypermediaActionParameterInfo:
                            this.AddParameterTypeRoute(httpGetHypermediaActionParameterInfo.RouteType,
                                httpGetHypermediaActionParameterInfo.Name!, HttpMethod.GET);
                            AddRouteKeyProducer(method, httpGetHypermediaActionParameterInfo);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(hypermediaAttribute),
                                $"Unknown hypermedia attribute: {hypermediaAttribute.GetType().Name}");
                    }
                }
            }
        }

        private void AddRouteKeyProducer<T>(MethodInfo method, T hypermediaAttribute)
            where T : HttpMethodAttribute, IHaveRouteInfo
        {
            var autoAddRouteKeyProducers = method.GetCustomAttribute<HttpGetAttribute>(true) is not null;
          
            if (hypermediaAttribute.RouteKeyProducerType != null)
            {
                if (typeof(HypermediaQueryResult).GetTypeInfo().IsAssignableFrom(hypermediaAttribute.RouteType))
                {
                    throw new RouteRegisterException(
                        $"Routes to Query's may not require a key '{hypermediaAttribute.RouteType}'. Queries should not be handled on a Entity.");
                }

                AttributedRouteHelper.AssertIsRouteKeyProducer(hypermediaAttribute.RouteKeyProducerType);
                var keyProducer = (IKeyProducer)Activator.CreateInstance(hypermediaAttribute.RouteKeyProducerType)!;
                this.AddRouteKeyProducer(hypermediaAttribute.RouteType, keyProducer);
            }
            else if (autoAddRouteKeyProducers && !typeof(HypermediaQueryResult).GetTypeInfo().IsAssignableFrom(hypermediaAttribute.RouteType))
            {
                var templateToUse = hypermediaAttribute.Template ?? string.Empty;
                var controllerRouteSegments = GetControllerRouteSegment(method);

                var template = TemplateParser.Parse(controllerRouteSegments + templateToUse);
                if (template.Parameters.Count > 0)
                {
                    this.AddRouteKeyProducer(
                        hypermediaAttribute.RouteType,
                        RouteKeyProducer.Create(hypermediaAttribute.RouteType, template.Parameters.Select(p => p.Name).ToList()));
                }
            }
        }

        private void AddRouteKeyProducer(MethodInfo method, IHypermediaObjectEndpointMetadata hypermediaAttribute, HttpMethodAttribute httpAttribute)
        {
            var autoAddRouteKeyProducers = httpAttribute is HttpGetAttribute;
          
            if (hypermediaAttribute.RouteKeyProducerType != null)
            {
                if (typeof(HypermediaQueryResult).GetTypeInfo().IsAssignableFrom(hypermediaAttribute.RouteType))
                {
                    throw new RouteRegisterException(
                        $"Routes to Query's may not require a key '{hypermediaAttribute.RouteType}'. Queries should not be handled on a Entity.");
                }

                AttributedRouteHelper.AssertIsRouteKeyProducer(hypermediaAttribute.RouteKeyProducerType);
                var keyProducer = (IKeyProducer)Activator.CreateInstance(hypermediaAttribute.RouteKeyProducerType)!;
                this.AddRouteKeyProducer(hypermediaAttribute.RouteType, keyProducer);
            }
            else if (autoAddRouteKeyProducers && !typeof(HypermediaQueryResult).GetTypeInfo().IsAssignableFrom(hypermediaAttribute.RouteType))
            {
                var templateToUse = httpAttribute.Template ?? string.Empty;
                var controllerRouteSegments = GetControllerRouteSegment(method);

                var template = TemplateParser.Parse(controllerRouteSegments + templateToUse);
                if (template.Parameters.Count > 0)
                {
                    this.AddRouteKeyProducer(
                        hypermediaAttribute.RouteType,
                        RouteKeyProducer.Create(hypermediaAttribute.RouteType, template.Parameters.Select(p => p.Name).ToList()));
                }
            }
        }

        private static void AssertAttributeHasName(HttpMethodAttribute hypermediaAttribute)
        {
            if (string.IsNullOrEmpty(hypermediaAttribute.Name))
            {
                throw new RouteRegisterException($"{hypermediaAttribute.GetType().Name} must have a name.");
            }
        }

        private static void AssertAttributeHasName(HypermediaEndpointAttribute hypermediaAttribute)
        {
            if (string.IsNullOrEmpty(hypermediaAttribute.EndpointName))
            {
                throw new RouteRegisterException($"{typeof(HypermediaEndpointAttribute).BeautifulName()} must have a name.");
            }
        }

        private static bool TryGetHypermediaEndpoint<T>(
            MethodInfo method,
            [NotNullWhen(true)] out T? hypermediaEndpointAttribute,
            [NotNullWhen(true)] out HttpMethodAttribute? httpAttribute)
            where T : Attribute
        {
            var httpAttributes = method.GetCustomAttributes<HttpMethodAttribute>(true).ToList();
            var hypermediaEndpoints = method.GetCustomAttributes<T>(true).ToList();

            if (httpAttributes is [] || hypermediaEndpoints is [])
            {
                hypermediaEndpointAttribute = null;
                httpAttribute = null;
                return false;
            }

            if (httpAttributes is { Count: > 1 })
            {
                throw new RouteRegisterException(
                    $"More than one {typeof(HttpMethodAttribute).BeautifulName()} on hypermedia endpoint {method.Name} is not supported.");
            }

            if (hypermediaEndpoints is { Count: > 1 })
            {
                throw new RouteRegisterException(
                    $"More than one {typeof(T).BeautifulName()} on endpoint {method.Name}");
            }


            hypermediaEndpointAttribute = hypermediaEndpoints[0];
            httpAttribute = httpAttributes[0];
            return true;
        }

        private static bool TryGetSingleHypermediaAttribute(MethodInfo method, [NotNullWhen(true)] out HttpMethodAttribute? hypermediaAttribute)
        {
            var httpMethodAttributes = method.GetCustomAttributes<HttpMethodAttribute>(true)
                .Where(IsLegacyExtensionAttribute)
                .ToArray();
            if (httpMethodAttributes.Length == 0)
            {
                hypermediaAttribute = null;
                return false;
            }

            if (httpMethodAttributes.Length > 1)
            {
                throw new RouteRegisterException($"More than one hypermedia attribute on route: {method.Name}");
            }

            hypermediaAttribute = httpMethodAttributes.First();
            return true;
        }
        
        private static bool IsLegacyExtensionAttribute(HttpMethodAttribute a)
        {
            return a is HttpGetHypermediaObject or HttpPostHypermediaAction or HttpPatchHypermediaAction
                or HttpPutHypermediaAction or HttpDeleteHypermediaAction or HttpGetHypermediaActionParameterInfo;
        }

        private string GetControllerRouteSegment(MethodInfo method)
        {
            var declaringType = method.DeclaringType;
            if (declaringType == null || !typeof(ControllerBase).IsAssignableFrom(declaringType))
            {
                // not a controller do not look for RouteAttribute, no segment
                return string.Empty;
            }
            
            var routeAttributeController = declaringType.GetCustomAttributes<RouteAttribute>().ToList();
            if (routeAttributeController.Count > 1)
            {
                logger.LogWarning($"Found more than one route attribute on Type {declaringType.Name}. Only first attribute will be used to automatically provide a RoutKeyProducer with a template. Using {routeAttributeController.First().Template}");
            } else if(routeAttributeController.Count == 0)
            {
                // no RouteAttribute 
                return string.Empty;
            }

            var template = routeAttributeController.First().Template;
            return template ?? string.Empty;
        }
    }
}