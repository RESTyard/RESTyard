using System;
using Microsoft.AspNetCore.Mvc;
using RESTyard.AspNetCore.WebApi.ExtensionMethods;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace RESTyard.AspNetCore.WebApi.AttributedRoutes
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