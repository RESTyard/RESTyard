using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IHypermediaUrlConfig defaultHypermediaUrlConfig;

        public RegisterRouteResolverFactory(
            IRouteRegister routeRegister,
            HypermediaExtensionsOptions hypermediaOptions,
            IRouteKeyFactory routeKeyFactory,
            IHypermediaUrlConfig defaultHypermediaUrlConfig)
        {
            this.routeRegister = routeRegister;
            this.hypermediaOptions = hypermediaOptions;
            this.routeKeyFactory = routeKeyFactory;
            this.defaultHypermediaUrlConfig = defaultHypermediaUrlConfig;
        }

        public IHypermediaRouteResolver CreateRouteResolver(HttpContext httpContext)
        {
            var hypermediaUrlConfig = HypermediaUrlConfigBuilder.Build(this.defaultHypermediaUrlConfig, httpContext.Request);
            var urlHelper = FormatterHelper.GetUrlHelperForCurrentContext(httpContext);
            var routeResolver = new RegisterRouteResolver(urlHelper, this.routeKeyFactory, this.routeRegister, this.hypermediaOptions, hypermediaUrlConfig);
            return routeResolver;
        }

        public IHypermediaRouteResolver CreateRouteResolver(IUrlHelper urlHelper)
        {
            return new RegisterRouteResolver(urlHelper, this.routeKeyFactory, routeRegister, hypermediaOptions, this.defaultHypermediaUrlConfig);
        }
    }


}