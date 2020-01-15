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
        public static IMvcBuilder AddHypermediaExtensions(this IMvcBuilder builder, IServiceCollection services, 
            HypermediaExtensionsOptions hypermediaOptions,
            params Assembly[] controllerAndHypermediaAssemblies)
        {
            //TODO: register ApplicationModel as singleton and use it everywhere to replace runtime reflection 
            builder.AddMvcOptions(o => 
                o.AddHypermediaExtensions(controllerAndHypermediaAssemblies: controllerAndHypermediaAssemblies)
                 .AddHypermediaParameterBinders(!hypermediaOptions.ImplicitHypermediaActionParameterBinders, controllerAndHypermediaAssemblies)
            );
            RegisterServices(services, hypermediaOptions, controllerAndHypermediaAssemblies);

            return builder;
        }

        public static IMvcCoreBuilder AddHypermediaExtensions(this IMvcCoreBuilder builder, IServiceCollection services,
            HypermediaExtensionsOptions hypermediaOptions,
            params Assembly[] controllerAndHypermediaAssemblies)
        {
            //TODO: register ApplicationModel as singleton and use it everywhere to replace runtime reflection 
            builder.AddMvcOptions(o =>
                o.AddHypermediaExtensions(controllerAndHypermediaAssemblies: controllerAndHypermediaAssemblies)
                 .AddHypermediaParameterBinders(!hypermediaOptions.ImplicitHypermediaActionParameterBinders, controllerAndHypermediaAssemblies)
            );
            RegisterServices(services, hypermediaOptions, controllerAndHypermediaAssemblies);

            return builder;
        }

        private static void RegisterServices(
            IServiceCollection services,
            HypermediaExtensionsOptions hypermediaOptions,
            Assembly[] controllerAndHypermediaAssemblies)
        {
            if (hypermediaOptions.AutoDeliverJsonSchemaForActionParameterTypes) { 
                services.AutoDeliverActionParameterSchemas(hypermediaOptions.CaseSensitiveParameterMatching, controllerAndHypermediaAssemblies);
            }

            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
        }

        /// <summary>
        /// Adds the Hypermedia Extensions.
        /// by default a Siren Formatters is added and
        /// the entry assembly is crawled for Hypermedia route attributes
        /// </summary>
        /// <param name="options">The options object of the MVC component.</param>
        /// <param name="alternateRouteRegister">If you wish to use another RoutRegister pass it here, also if you wish another assembly to be crawled.</param>
        /// <param name="alternateQueryStringBuilder">Provide an alternate QueryStringBuilder used for building URL's.</param>
        /// <param name="hypermediaUrlConfig">Configures the URL used in Hypermedia responses.</param>
        /// <param name="hypermediaConverterConfiguration">Configures the creation of Hypermedia documents.</param>
        /// <param name="hypermediaOptions">Configures general options for teh extensions.</param>
        /// <param name="controllerAndHypermediaAssemblies">Assemblies to crawl for controller routes and hypermedia objects</param>
        private static MvcOptions AddHypermediaExtensions(
            this MvcOptions options,
            IRouteRegister alternateRouteRegister = null,
            IQueryStringBuilder alternateQueryStringBuilder = null,
            IHypermediaUrlConfig hypermediaUrlConfig = null,
            IHypermediaConverterConfiguration hypermediaConverterConfiguration = null,
            HypermediaExtensionsOptions hypermediaOptions = null,
            params Assembly[] controllerAndHypermediaAssemblies)
        {
            hypermediaOptions = hypermediaOptions ?? new HypermediaExtensionsOptions();
            var routeRegister = alternateRouteRegister ?? new AttributedRoutesRegister(controllerAndHypermediaAssemblies);

            var routeResolverFactory = new RegisterRouteResolverFactory(routeRegister, hypermediaOptions);
            var routeKeyFactory = new RouteKeyFactory(routeRegister);

            var queryStringBuilder = alternateQueryStringBuilder ?? new QueryStringBuilder();
            var hypermediaQueryLocationFormatter = new HypermediaQueryLocationFormatter(routeResolverFactory, routeKeyFactory, queryStringBuilder, hypermediaUrlConfig);
            var hypermediaEntityLocationFormatter = new HypermediaEntityLocationFormatter(routeResolverFactory, routeKeyFactory, hypermediaUrlConfig);

            var sirenHypermediaConverterFactory = new SirenHypermediaConverterFactory(queryStringBuilder, hypermediaConverterConfiguration?.SirenConverterConfiguration);
            var sirenHypermediaFormatter = new SirenHypermediaFormatter(routeResolverFactory, routeKeyFactory, sirenHypermediaConverterFactory, hypermediaUrlConfig);

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

                return hmoType.GetHmoMethods.Select(_ => _.RouteTemplateFull).ToImmutableArray();
            }, forAttributedActionParametersOnly));

            return options;
        }

        /// <summary>
        /// Automatically deliver NJson schema for hypermedia action parameters. Custom schemas can still be delivered by implementing controller methods attributed
        /// with <see cref="HttpGetHypermediaActionParameterInfo"/> attibute.
        /// </summary>
        /// <param name="serviceCollection"></param>
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