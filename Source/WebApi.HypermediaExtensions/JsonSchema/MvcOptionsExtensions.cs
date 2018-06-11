using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using WebApi.HypermediaExtensions.Hypermedia.Actions;
using WebApi.HypermediaExtensions.Util;
using WebApi.HypermediaExtensions.WebApi.Controller;
using WebApi.HypermediaExtensions.WebApi.RouteResolver;

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
            var applicationModel = ApplicationModel.Create(controllerAssemblies);

            options.ModelBinderProviders.Insert(0, new HypermediaParameterFromBodyBinderProvider(t =>
            {
                if (!applicationModel.HmoTypes.TryGetValue(t, out var hmoType))
                {
                    throw new ArgumentException($"No route found for type {t.BeautifulName()}");
                }

                return hmoType.GetHmoMethod.RouteTemplateFull;
            }, forAttributedActionParametersOnly));

            return options;
        }

        public static IServiceCollection RegisterActionTypeController(this IServiceCollection serviceCollection)
        {
            var applicationModel = ApplicationModel.Create();
            var controller = new ActionParameterSchemas(applicationModel.ActionParameterTypes.Values.Select(_ => _.Type));
            return serviceCollection.AddSingleton(controller);
        }
    }
}