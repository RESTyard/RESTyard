using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using WebApi.HypermediaExtensions.Hypermedia.Actions;
using WebApi.HypermediaExtensions.Util;
using WebApi.HypermediaExtensions.WebApi.AttributedRoutes;

namespace WebApi.HypermediaExtensions.JsonSchema
{
    public static class MvcOptionsExtensions
    {
        /// <summary>
        /// Add custom binder for parameters of hypermedia actions that derive from <see cref="IHypermediaActionParameter"/>. 
        /// Enables usage of <see cref="KeyFromUriAttribute"/> for properties of those parameter types. 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="forAttributedActionParametersOnly">
        ///     If set custom binder will be used for all parameter types that are not attributed differently. 
        ///     If set to false custom binder will be used for parameter types explicitly attributed with <see cref="HypermediaActionParameterFromBodyAttribute"/> only.
        ///  </param>
        /// <param name="controllerAssemblies"></param>
        /// <returns></returns>
        public static MvcOptions AddHypermediaParameterBinders(this MvcOptions options, bool forAttributedActionParametersOnly = false, params Assembly[] controllerAssemblies)
        {
            controllerAssemblies = controllerAssemblies.Any() ? controllerAssemblies : new[] { Assembly.GetEntryAssembly() };

            var controllerMethods = controllerAssemblies
                .SelectMany(a => a.GetTypes()
                    .Select(t => t.GetTypeInfo())
                    .Where(t => typeof(Controller).GetTypeInfo().IsAssignableFrom(t))
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


            options.ModelBinderProviders.Insert(0, new HypermediaParameterFromBodyBinderProvider(t =>
            {
                if (!routesByHmoType.TryGetValue(t, out string template))
                {
                    throw new ArgumentException($"No route found for type {t.BeautifulName()}");
                }

                return template;
            }, forAttributedActionParametersOnly));

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