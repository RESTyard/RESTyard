using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Logging;
using WebApi.HypermediaExtensions.Exceptions;
using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.Util.Extensions;
using WebApi.HypermediaExtensions.WebApi.AttributedRoutes;
using WebApi.HypermediaExtensions.WebApi.ExtensionMethods;

namespace WebApi.HypermediaExtensions.WebApi.RouteResolver
{
    public class AttributedRoutesRegister : RouteRegister
    {
        private readonly ILogger logger;

        public AttributedRoutesRegister(HypermediaExtensionsOptions hypermediaOptions, ILogger<AttributedRoutesRegister> logger)
        {
            this.logger = logger;
            var assembliesToCrawl = hypermediaOptions.ControllerAndHypermediaAssemblies.Length > 0
                ? hypermediaOptions.ControllerAndHypermediaAssemblies
                : Assembly.GetEntryAssembly().Yield();

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
                AddAttributedRoute<HttpGetHypermediaObject>(method, HttpMethod.GET, this.AddHypermediaObjectRoute, true);
                AddAttributedRoute<HttpPostHypermediaAction>(method, HttpMethod.POST, this.AddActionRoute);
                AddAttributedRoute<HttpDeleteHypermediaAction>(method, HttpMethod.DELETE, this.AddActionRoute);
                AddAttributedRoute<HttpPatchHypermediaAction>(method, HttpMethod.PATCH, this.AddActionRoute);
                AddAttributedRoute<HttpGetHypermediaActionParameterInfo>(method, HttpMethod.GET, this.AddParameterTypeRoute);
            }
        }

        private void AddAttributedRoute<T>(MethodInfo method, HttpMethod httpMethod, Action<Type, string, HttpMethod> addAction, bool autoAddRouteKeyProducers = false) where T : HttpMethodAttribute, IHaveRouteInfo
        {
            var attribute = method.GetCustomAttribute<T>();
            if (attribute == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(attribute.Name))
            {
                throw new RouteRegisterException($"{typeof(T).Name} must have a name.");
            }

            addAction(attribute.RouteType, attribute.Name, httpMethod);

            if (attribute.RouteKeyProducerType != null)
            {
                if (typeof(HypermediaQueryResult).GetTypeInfo().IsAssignableFrom(attribute.RouteType))
                {
                    throw new RouteRegisterException($"Routes to Query's may not require a key '{attribute.RouteType}'. Queries should not be handled on a Entity.");
                }

                var keyProducer = (IKeyProducer)Activator.CreateInstance(attribute.RouteKeyProducerType);
                this.AddRouteKeyProducer(attribute.RouteType, keyProducer);
            }
            else if (autoAddRouteKeyProducers && !typeof(HypermediaQueryResult).GetTypeInfo().IsAssignableFrom(attribute.RouteType))
            {
                var templateToUse = attribute.Template ?? string.Empty;
                var controllerRouteSegments = GetControllerRouteSegment(method);

                var template = TemplateParser.Parse(controllerRouteSegments + templateToUse);
                if (template.Parameters.Count > 0)
                {
                    this.AddRouteKeyProducer(attribute.RouteType, RouteKeyProducer.Create(attribute.RouteType, template.Parameters.Select(p => p.Name).ToList()));
                }
            }
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