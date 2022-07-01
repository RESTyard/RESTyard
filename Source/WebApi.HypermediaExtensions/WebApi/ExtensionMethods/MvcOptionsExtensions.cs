using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebApi.HypermediaExtensions.Hypermedia.Actions;
using WebApi.HypermediaExtensions.JsonSchema;
using WebApi.HypermediaExtensions.Query;
using WebApi.HypermediaExtensions.Util;
using WebApi.HypermediaExtensions.WebApi.AttributedRoutes;
using WebApi.HypermediaExtensions.WebApi.Controller;
using WebApi.HypermediaExtensions.WebApi.Formatter;
using WebApi.HypermediaExtensions.WebApi.RouteResolver;

namespace WebApi.HypermediaExtensions.WebApi.ExtensionMethods
{
    public static class MvcOptionsExtensions
    {
        /// <summary>
        /// Adds the Hypermedia Extensions.
        /// By default a Siren Formatters is added and the entry assembly is crawled for Hypermedia route attributes
        /// </summary>
        public static IServiceCollection AddHypermediaExtensions(this IServiceCollection serviceCollection, HypermediaExtensionsOptions hypermediaOptions)
        {
            if (hypermediaOptions.AutoDeliverJsonSchemaForActionParameterTypes)
            {
                serviceCollection.AutoDeliverActionParameterSchemas(hypermediaOptions.CaseSensitiveParameterMatching, hypermediaOptions.ControllerAndHypermediaAssemblies);
            }
            serviceCollection.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
            
            serviceCollection.AddSingleton(hypermediaOptions);
            serviceCollection.Configure<MvcOptions>(options =>
            {
                options.AddHypermediaExtensionsInternal(hypermediaOptions: hypermediaOptions);
                options.AddHypermediaParameterBinders(hypermediaOptions);
            });
            return serviceCollection;
        }

        /// <summary>
        /// Adds the Hypermedia Extension Formatters. This is a infrastructure method to be used to change core behaviour of the extensions.
        /// For default cases consider using the convenience overloads AddHypermediaExtensions.
        /// By default a Siren Formatters is added and the entry assembly is crawled for Hypermedia route attributes
        /// </summary>
        /// <param name="options">The options object of the MVC component.</param>
        /// <param name="alternateRouteRegister">If you wish to use another RoutRegister pass it here, also if you wish another assembly to be crawled.</param>
        /// <param name="alternateQueryStringBuilder">Provide an alternate QueryStringBuilder used for building URL's.</param>
        /// <param name="hypermediaOptions">Configures general options for teh extensions.</param>
        public static MvcOptions AddHypermediaExtensionsInternal(
            this MvcOptions options,
            IRouteRegister alternateRouteRegister = null,
            IQueryStringBuilder alternateQueryStringBuilder = null,
            HypermediaExtensionsOptions hypermediaOptions = null)
        {
            hypermediaOptions = hypermediaOptions ?? new HypermediaExtensionsOptions();
            var routeRegister = alternateRouteRegister ?? new AttributedRoutesRegister(hypermediaOptions.ControllerAndHypermediaAssemblies);

            var routeResolverFactory = new RegisterRouteResolverFactory(routeRegister, hypermediaOptions);
            var routeKeyFactory = new RouteKeyFactory(routeRegister);

            var queryStringBuilder = alternateQueryStringBuilder ?? new QueryStringBuilder();
            var hypermediaQueryLocationFormatter = new HypermediaQueryLocationFormatter(routeResolverFactory, routeKeyFactory, queryStringBuilder, hypermediaOptions.HypermediaUrlConfig);
            var hypermediaEntityLocationFormatter = new HypermediaEntityLocationFormatter(routeResolverFactory, routeKeyFactory, hypermediaOptions.HypermediaUrlConfig);

            var sirenHypermediaConverterFactory = new SirenHypermediaConverterFactory(queryStringBuilder, hypermediaOptions.HypermediaConverterConfiguration);
            var sirenHypermediaFormatter = new SirenHypermediaFormatter(routeResolverFactory, routeKeyFactory, sirenHypermediaConverterFactory, hypermediaOptions.HypermediaUrlConfig);

            options.OutputFormatters.Insert(0, hypermediaQueryLocationFormatter);
            options.OutputFormatters.Insert(0, hypermediaEntityLocationFormatter);
            options.OutputFormatters.Insert(0, sirenHypermediaFormatter);

            return options;
        }

        /// <summary>
        /// Add custom binder for parameters of hypermedia actions that derive from <see cref="IHypermediaActionParameter"/>. 
        /// Enables usage of <see cref="KeyFromUriAttribute"/> for properties of those parameter types. 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="hypermediaOptions"></param>
        /// <returns></returns>
        public static MvcOptions AddHypermediaParameterBinders(this MvcOptions options, HypermediaExtensionsOptions hypermediaOptions)
        {
            var forAttributedActionParametersOnly = !hypermediaOptions.ImplicitHypermediaActionParameterBinders;
            var applicationModel = ApplicationModel.Create(hypermediaOptions.ControllerAndHypermediaAssemblies);

            options.ModelBinderProviders.Insert(0, new HypermediaParameterFromBodyBinderProvider(t =>
            {
                if (!applicationModel.HmoTypes.TryGetValue(t, out var hmoType))
                {
                    throw new ArgumentException($"No route found for type {t.BeautifulName()}");
                }

                return hmoType.GetHmoMethods.Select(_ => _.RouteTemplateFull).ToImmutableArray();
            }, forAttributedActionParametersOnly));

            return options;
        }

        /// <summary>
        /// Automatically deliver NJson schema for hypermedia action parameters. Custom schemas can still be delivered by implementing controller methods attributed
        /// with <see cref="HttpGetHypermediaActionParameterInfo"/> attibute.
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="useCaseSensitiveParameterMatching"></param>
        /// <param name="controllerAssemblies"></param>
        /// <returns></returns>
        public static IServiceCollection AutoDeliverActionParameterSchemas(this IServiceCollection serviceCollection, bool useCaseSensitiveParameterMatching, params Assembly[] controllerAssemblies)
        {
            var applicationModel = ApplicationModel.Create(controllerAssemblies);
            var controller = new ActionParameterSchemas(applicationModel.ActionParameterTypes.Values.Select(_ => _.Type), useCaseSensitiveParameterMatching);
            return serviceCollection.AddSingleton(controller);
        }
    }
}