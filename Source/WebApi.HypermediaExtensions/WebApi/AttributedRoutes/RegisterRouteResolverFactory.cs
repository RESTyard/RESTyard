using System;
using Microsoft.AspNetCore.Mvc;
using RESTyard.WebApi.Extensions.WebApi.ExtensionMethods;
using RESTyard.WebApi.Extensions.WebApi.RouteResolver;

namespace RESTyard.WebApi.Extensions.WebApi.AttributedRoutes
{
    // required because theUrlHelper changes with every context.
    public class RegisterRouteResolverFactory : IRouteResolverFactory
    {
        private readonly IRouteRegister routeRegister;
        private readonly HypermediaExtensionsOptions hypermediaOptions;

        public RegisterRouteResolverFactory(IRouteRegister routeRegister, HypermediaExtensionsOptions hypermediaOptions)
        {
            this.routeRegister = routeRegister;
            this.hypermediaOptions = hypermediaOptions;
        }

        public IHypermediaRouteResolver CreateRouteResolver(IUrlHelper urlHelper, IRouteKeyFactory routeKeyFactory, IHypermediaUrlConfig hypermediaUrlConfig = null)
        {
            return new RegisterRouteResolver(urlHelper, routeKeyFactory, routeRegister, hypermediaOptions, hypermediaUrlConfig);
        }
    }


}