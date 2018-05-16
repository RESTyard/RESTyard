﻿using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing.Template;
using WebApiHypermediaExtensionsCore.Exceptions;
using WebApiHypermediaExtensionsCore.Hypermedia;
using WebApiHypermediaExtensionsCore.WebApi.AttributedRoutes;

namespace WebApiHypermediaExtensionsCore.WebApi.RouteResolver
{
    public class AttributedRoutesRegister : RouteRegister
    {
        public AttributedRoutesRegister(Assembly assembly = null)
        {
            var assemblyToCrawl = assembly ?? Assembly.GetEntryAssembly();
            RegisterAssemblyRoutes(assemblyToCrawl);
        }

        private void RegisterAssemblyRoutes(Assembly assembly)
        {
            var assemblyMethods = assembly.GetTypes().SelectMany(t => t.GetMethods());
            foreach (var method in assemblyMethods)
            {
                AddAttributedRoute<HttpGetHypermediaObject>(method, this.AddHypermediaObjectRoute);
                AddAttributedRoute<HttpPostHypermediaAction>(method, this.AddActionRoute);
                AddAttributedRoute<HttpGetHypermediaActionParameterInfo>(method, this.AddParameterTypeRoute);
            }
        }

        private void AddAttributedRoute<T>(MethodInfo method, Action<Type, string> addAction) where T : HttpMethodAttribute, IHaveRouteInfo
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

            addAction(attribute.RouteType, attribute.Name);

            if (attribute.RouteKeyProducerType != null)
            {
                if (typeof(HypermediaQueryResult).IsAssignableFrom(attribute.RouteType))
                {
                    throw new RouteRegisterException($"Routes to Querys may not require a key '{attribute.RouteType}'. Queries should not be handled on a Entity.");
                }

                var keyProducer = (IKeyProducer)Activator.CreateInstance(attribute.RouteKeyProducerType);
                this.AddRouteKeyProducer(attribute.RouteType, keyProducer);
            }
            else if (!typeof(HypermediaQueryResult).IsAssignableFrom(attribute.RouteType) && attribute.Template != null)
            {
                var template = TemplateParser.Parse(attribute.Template);
                if (template.Parameters.Count > 0)
                {
                    //TODO: how to handle multiple parameters
                    this.AddRouteKeyProducer(attribute.RouteType, new RouteKeyProducer(template.Parameters[0].Name));
                }
            }
        }
    }
}