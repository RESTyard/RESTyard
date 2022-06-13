using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing.Template;
using WebApi.HypermediaExtensions.Exceptions;
using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.Util.Extensions;
using WebApi.HypermediaExtensions.WebApi.AttributedRoutes;

namespace WebApi.HypermediaExtensions.WebApi.RouteResolver
{
    public class AttributedRoutesRegister : RouteRegister
    {
        public AttributedRoutesRegister(params Assembly[] controllerAndHypermediaAssemblies)
        {
            var assembliesToCrawl = controllerAndHypermediaAssemblies.Length > 0
                ? controllerAndHypermediaAssemblies
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
                    throw new RouteRegisterException($"Routes to Querys may not require a key '{attribute.RouteType}'. Queries should not be handled on a Entity.");
                }

                var keyProducer = (IKeyProducer)Activator.CreateInstance(attribute.RouteKeyProducerType);
                this.AddRouteKeyProducer(attribute.RouteType, keyProducer);
            }
            else if (autoAddRouteKeyProducers && !typeof(HypermediaQueryResult).GetTypeInfo().IsAssignableFrom(attribute.RouteType) && attribute.Template != null)
            {
                var template = TemplateParser.Parse(attribute.Template);
                if (template.Parameters.Count > 0)
                {
                    this.AddRouteKeyProducer(attribute.RouteType, RouteKeyProducer.Create(attribute.RouteType, template.Parameters.Select(p => p.Name).ToList()));
                }
            }
        }
    }
}