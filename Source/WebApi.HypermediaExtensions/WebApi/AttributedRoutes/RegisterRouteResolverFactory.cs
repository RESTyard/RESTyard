using Microsoft.AspNetCore.Mvc;
using WebApi.HypermediaExtensions.WebApi.ExtensionMethods;
using WebApi.HypermediaExtensions.WebApi.RouteResolver;

namespace WebApi.HypermediaExtensions.WebApi.AttributedRoutes
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

        public IHypermediaRouteResolver CreateRouteResolver(IUrlHelper urlHelper, IRouteKeyFactory routeKeyFactory, HypermediaUrlConfig hypermediaUrlConfig = null)
        {
            return new RegisterRouteResolver(urlHelper, routeKeyFactory, routeRegister, hypermediaOptions, hypermediaUrlConfig);
        }
    }


}