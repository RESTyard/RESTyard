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
                : Assembly.GetEntryAssembly()?.Yield() ?? Enumerable.Empty<Assembly>();

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
                if (!TryGetSingleHypermediaAttribute(method, out var hypermediaAttribute))
                {
                    continue;
                }
                
                AssertAttributeHasName(hypermediaAttribute);

                switch (hypermediaAttribute)
                {
                    case HttpGetHypermediaObject httpGetHypermediaObject:
                        this.AddHypermediaObjectRoute(httpGetHypermediaObject.RouteType, httpGetHypermediaObject.Name, HttpMethod.GET);
                        AddRouteKeyProducer(method, httpGetHypermediaObject);
                        break;
                    case HttpPostHypermediaAction httpPostHypermediaAction:
                        this.AddActionRoute(httpPostHypermediaAction.RouteType, httpPostHypermediaAction.Name, HttpMethod.POST, httpPostHypermediaAction.AcceptedMediaType);
                        AddRouteKeyProducer(method, httpPostHypermediaAction);
                        break;
                    case HttpPutHypermediaAction httpPutHypermediaAction:
                        this.AddActionRoute(httpPutHypermediaAction.RouteType, httpPutHypermediaAction.Name, HttpMethod.PUT, httpPutHypermediaAction.AcceptedMediaType);
                        AddRouteKeyProducer(method, httpPutHypermediaAction);
                        break;
                    case HttpDeleteHypermediaAction httpDeleteHypermediaAction:
                        this.AddActionRoute(httpDeleteHypermediaAction.RouteType, httpDeleteHypermediaAction.Name, HttpMethod.DELETE, httpDeleteHypermediaAction.AcceptedMediaType);
                        AddRouteKeyProducer(method, httpDeleteHypermediaAction);
                        break;
                    case HttpPatchHypermediaAction httpPatchHypermediaAction:
                        this.AddActionRoute(httpPatchHypermediaAction.RouteType, httpPatchHypermediaAction.Name, HttpMethod.PATCH, httpPatchHypermediaAction.AcceptedMediaType);
                        AddRouteKeyProducer(method, httpPatchHypermediaAction);
                        break;
                    case HttpGetHypermediaActionParameterInfo httpGetHypermediaActionParameterInfo:
                        this.AddParameterTypeRoute(httpGetHypermediaActionParameterInfo.RouteType, httpGetHypermediaActionParameterInfo.Name, HttpMethod.GET);
                        AddRouteKeyProducer(method, httpGetHypermediaActionParameterInfo);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(hypermediaAttribute), $"Unknown hypermedia attribute: {hypermediaAttribute.GetType().Name}");
                }
            }
        }

        private void AddRouteKeyProducer<T>(MethodInfo method, T hypermediaAttribute)
            where T : HttpMethodAttribute, IHaveRouteInfo
        {
            var autoAddRouteKeyProducers = hypermediaAttribute is HttpGetHypermediaObject;
          
            if (hypermediaAttribute.RouteKeyProducerType != null)
            {
                if (typeof(HypermediaQueryResult).GetTypeInfo().IsAssignableFrom(hypermediaAttribute.RouteType))
                {
                    throw new RouteRegisterException(
                        $"Routes to Query's may not require a key '{hypermediaAttribute.RouteType}'. Queries should not be handled on a Entity.");
                }

                var keyProducer = (IKeyProducer)Activator.CreateInstance(hypermediaAttribute.RouteKeyProducerType);
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

        private static void AssertAttributeHasName(HttpMethodAttribute hypermediaAttribute)
        {
            if (string.IsNullOrEmpty(hypermediaAttribute.Name))
            {
                throw new RouteRegisterException($"{hypermediaAttribute.GetType().Name} must have a name.");
            }
        }

        private static bool TryGetSingleHypermediaAttribute(MethodInfo method, [NotNullWhen(true)] out HttpMethodAttribute? hypermediaAttribute)
        {
            var httpMethodAttributes = method.GetCustomAttributes<HttpMethodAttribute>(true)
                .Where(IsExtensionAttribute)
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
        
        private static bool IsExtensionAttribute(HttpMethodAttribute a)
        {
            var attributeType = a.GetType();
            return attributeType == typeof(HttpGetHypermediaObject)
                   || attributeType == typeof(HttpPutHypermediaAction)
                   || attributeType == typeof(HttpPostHypermediaAction)
                   || attributeType == typeof(HttpDeleteHypermediaAction)
                   || attributeType == typeof(HttpPatchHypermediaAction)
                   || attributeType == typeof(HttpGetHypermediaActionParameterInfo);
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