using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using RESTyard.AspNetCore.Exceptions;
using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.AspNetCore.JsonSchema;
using RESTyard.AspNetCore.Query;
using RESTyard.AspNetCore.Util;
using RESTyard.AspNetCore.WebApi.AttributedRoutes;
using RESTyard.AspNetCore.WebApi.Controller;
using RESTyard.AspNetCore.WebApi.Formatter;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace RESTyard.AspNetCore.WebApi.ExtensionMethods
{
    public static class StartupExtensions
    {
        /// <summary>
        /// Adds the Hypermedia Extensions.
        /// By default a Siren Formatters is added and the entry assembly is crawled for Hypermedia route attributes
        /// </summary>
        public static IServiceCollection AddHypermediaExtensions(this IServiceCollection serviceCollection, Action<HypermediaExtensionsOptions>? configureHypermediaOptionsAction = null)
        {
            var hypermediaOptions = new HypermediaExtensionsOptions();
            configureHypermediaOptionsAction?.Invoke(hypermediaOptions);
            
            serviceCollection.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
            serviceCollection.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            serviceCollection.AddSingleton(hypermediaOptions);
            serviceCollection.AddSingleton(CreateApplicationModel);
            serviceCollection.AddSingletonWithAlternative<IRouteRegister, AttributedRoutesRegister>(hypermediaOptions.AlternateRouteRegister);
            serviceCollection.AddSingletonWithAlternative<IQueryStringBuilder, QueryStringBuilder>(hypermediaOptions.AlternateQueryStringBuilder);
            serviceCollection.AddSingleton<IRouteResolverFactory, RegisterRouteResolverFactory>();
            serviceCollection.AddScoped<IHypermediaRouteResolver>(sp =>
            {
                var factory = sp.GetRequiredService<IRouteResolverFactory>();
                var accessor = sp.GetRequiredService<IHttpContextAccessor>();
                var httpContext = accessor.HttpContext;
                if (httpContext is null)
                {
                    throw new HypermediaException(
                        "Cannot resolve HttpContext from a scope that is not created as part of a request");
                }
                return factory.CreateRouteResolver(httpContext);
            });
            serviceCollection.AddSingleton<IRouteKeyFactory, RouteKeyFactory>();
            serviceCollection.AddSingleton<ISirenHypermediaConverterFactory, SirenHypermediaConverterFactory>();
            serviceCollection.AddSingleton<HypermediaQueryLocationFormatter>();
            serviceCollection.AddSingleton<HypermediaEntityLocationFormatter>();
            serviceCollection.AddSingleton<SirenHypermediaFormatter>();
            
            if (hypermediaOptions.AutoDeliverJsonSchemaForActionParameterTypes)
            {
                serviceCollection.AddSingleton<ActionParameterSchemas>();
            }
           
            serviceCollection.ConfigureOptions<ConfigureMvcOptionsForHypermediaExtensions>();
            return serviceCollection;
        }

        internal class ConfigureMvcOptionsForHypermediaExtensions : IConfigureOptions<MvcOptions>
        {
            private readonly HypermediaExtensionsOptions hypermediaOptions;
            private readonly ApplicationModel applicationModel;
            private readonly HypermediaQueryLocationFormatter hypermediaQueryLocationFormatter;
            private readonly HypermediaEntityLocationFormatter hypermediaEntityLocationFormatter;
            private readonly SirenHypermediaFormatter sirenHypermediaFormatter;

            public ConfigureMvcOptionsForHypermediaExtensions(
                HypermediaExtensionsOptions hypermediaOptions, 
                ApplicationModel applicationModel,
                HypermediaQueryLocationFormatter hypermediaQueryLocationFormatter,
                HypermediaEntityLocationFormatter hypermediaEntityLocationFormatter,
                SirenHypermediaFormatter sirenHypermediaFormatter)
            {
                this.hypermediaOptions = hypermediaOptions;
                this.applicationModel = applicationModel;
                this.hypermediaQueryLocationFormatter = hypermediaQueryLocationFormatter;
                this.hypermediaEntityLocationFormatter = hypermediaEntityLocationFormatter;
                this.sirenHypermediaFormatter = sirenHypermediaFormatter;
            }

            public void Configure(MvcOptions options)
            {
                options.AddHypermediaExtensionsOutputFormatters(hypermediaQueryLocationFormatter, hypermediaEntityLocationFormatter, sirenHypermediaFormatter);
                options.AddHypermediaParameterBinders(hypermediaOptions, applicationModel);
            }
        }

        private static ApplicationModel CreateApplicationModel(IServiceProvider s)
        {
            var hypermediaOptions = s.GetRequiredService<HypermediaExtensionsOptions>();
            return ApplicationModel.Create(hypermediaOptions.ControllerAndHypermediaAssemblies);
        }

        /// <summary>
        /// Adds the Hypermedia Extension Formatters. This is a infrastructure method to be used to change core behaviour of the extensions.
        /// For default cases consider using the convenience overloads AddHypermediaExtensions.
        /// By default a Siren Formatters is added and the entry assembly is crawled for Hypermedia route attributes
        /// </summary>
        public static MvcOptions AddHypermediaExtensionsOutputFormatters(
            this MvcOptions options,
            HypermediaQueryLocationFormatter hypermediaQueryLocationFormatter,
            HypermediaEntityLocationFormatter hypermediaEntityLocationFormatter,
            SirenHypermediaFormatter sirenHypermediaFormatter
            )
        {
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
        /// <param name="applicationModel">Model of the HTO objects and controllers</param>
        /// <returns></returns>
        public static MvcOptions AddHypermediaParameterBinders(this MvcOptions options, HypermediaExtensionsOptions hypermediaOptions, ApplicationModel applicationModel)
        {
            var forAttributedActionParametersOnly = !hypermediaOptions.ImplicitHypermediaActionParameterBinders;

            options.ModelBinderProviders.Insert(0, new HypermediaParameterFromBodyBinderProvider(t =>
            {
                if (!applicationModel.HmoTypes.TryGetValue(t, out var hmoType))
                {
                    throw new ArgumentException($"No route found for type {t.BeautifulName()}");
                }

                return hmoType.GetHmoMethods.Select(_ => _.RouteTemplateFull).ToImmutableArray();
            }, forAttributedActionParametersOnly));
            options.ModelBinderProviders.Insert(
                1,
                new HypermediaParameterFromFormBinderProvider(
                    t =>
                    {
                        if (!applicationModel.HmoTypes.TryGetValue(t, out var hmoType))
                        {
                            throw new ArgumentException($"No route found for type {t.BeautifulName()}");
                        }

                        return hmoType.GetHmoMethods.Select(_ => _.RouteTemplateFull).ToImmutableArray();
                    },
                    forAttributedActionParametersOnly));

            return options;
        }

        public static IServiceCollection AddSingletonWithAlternative<TInterface, TDefault>(this IServiceCollection serviceCollection, Type? alternative)
            where TDefault : TInterface
        {
            var serviceType = typeof(TInterface);
            if (alternative != null && !serviceType.IsAssignableFrom(alternative))
            {
                throw new Exception($"Provided type as alternative for {serviceType.Name} does not implement the interface");
            }

            serviceCollection.AddSingleton(serviceType, alternative ?? typeof(TDefault));

            return serviceCollection;
        }
    }
}