using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using WebApi.HypermediaExtensions.Util;
using WebApi.HypermediaExtensions.WebApi.AttributedRoutes;

namespace WebApi.HypermediaExtensions.JsonSchema
{
    public static class MvcOptionsExtensions
    {
        public static MvcOptions AddHypermediaParameterBinders(this MvcOptions options, params Assembly[] controllerAssemblies)
        {
            controllerAssemblies = controllerAssemblies.Any() ? controllerAssemblies : new[] { Assembly.GetEntryAssembly() };

            var controllerMethods = controllerAssemblies
                .SelectMany(a => a.GetTypes()
                    .Select(t => t.GetTypeInfo())
                    .Select(t => new { t, methods = t.GetMethods() }))
                .ToImmutableArray();

            var getHmoMethods = controllerMethods
                .Select(_ => new
                {
                    _.t,
                    methods = _.methods.Select(m => new
                    {
                        m,
                        att = m.GetCustomAttribute<HttpGetHypermediaObject>()
                    }).Where(m => m.att != null)
                });

            var routesByHmoType = getHmoMethods
                .SelectMany(_ => _.methods)
                .ToDictionary(m => m.att.RouteType, m =>
                {
                    var controllerTemplate = m.m.DeclaringType.GetTypeInfo().GetCustomAttribute<RouteAttribute>()?.Template;
                    return string.Concat(controllerTemplate, "/", m.att.Template).Replace("//", "/").Replace("///", "/");
                }).ToImmutableDictionary();


            options.ModelBinderProviders.Insert(0, new FromBodyHypermediaParameterBinderProvider(t =>
            {
                if (!routesByHmoType.TryGetValue(t, out string template))
                {
                    throw new ArgumentException($"No route found for type {t.BeautifulName()}");
                }

                return template;
            }));

            return options;


            //var hypermediaActionMethods = controllerMethods
            //    .Select(_ => new
            //    {
            //        _.t,
            //        methods = _.methods.Select(m => new
            //        {
            //            m,
            //            att = m.GetCustomAttribute<HttpPostHypermediaAction>()
            //        }).Where(m => m.att != null)
            //    }).ToImmutableArray();

            //var hypermediaGetParameterInfoMethods = controllerMethods
            //    .Select(_ => new
            //    {
            //        _.t,
            //        methods = _.methods.Select(m => new
            //        {
            //            m,
            //            att = m.GetCustomAttribute<HttpGetHypermediaActionParameterInfo>()
            //        }).Where(m => m.att != null)
            //    });
        }
    }
}