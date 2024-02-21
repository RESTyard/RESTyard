using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using RESTyard.AspNetCore.WebApi.ExtensionMethods;
using RESTyard.AspNetCore.WebApi.Formatter;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace RESTyard.AspNetCore.WebApi.AttributedRoutes
{
    // required because theUrlHelper changes with every context.
    public class RegisterRouteResolverFactory : IRouteResolverFactory
    {
        private readonly IRouteRegister routeRegister;
        private readonly HypermediaExtensionsOptions hypermediaOptions;
        private readonly IRouteKeyFactory routeKeyFactory;
        private static readonly IUrlHelperFactory UrlHelperFactory = new UrlHelperFactory();

        public RegisterRouteResolverFactory(
            IRouteRegister routeRegister,
            HypermediaExtensionsOptions hypermediaOptions,
            IRouteKeyFactory routeKeyFactory)
        {
            this.routeRegister = routeRegister;
            this.hypermediaOptions = hypermediaOptions;
            this.routeKeyFactory = routeKeyFactory;
        }

        public IHypermediaRouteResolver CreateRouteResolver(HttpContext httpContext)
        {
            var hypermediaUrlConfig = HypermediaUrlConfigBuilder.Build(httpContext.Request);
            var urlHelper = CreateUrlHelper(httpContext);
            var routeResolver = new RegisterRouteResolver(urlHelper, this.routeKeyFactory, this.routeRegister, this.hypermediaOptions, hypermediaUrlConfig);
            return routeResolver;
        }

        private static IUrlHelper CreateUrlHelper(HttpContext httpContext)
        {
            var actionContext = httpContext.RequestServices.GetRequiredService<IActionContextAccessor>().ActionContext!;
            var urlHelper = UrlHelperFactory.GetUrlHelper(actionContext);
            return urlHelper;
        }

        public IHypermediaRouteResolver CreateRouteResolver(IUrlHelper urlHelper, IHypermediaUrlConfig urlConfig)
        {
            return new RegisterRouteResolver(urlHelper, this.routeKeyFactory, routeRegister, hypermediaOptions, urlConfig);
        }
    }


}