using Microsoft.AspNetCore.Mvc;
using WebApiHypermediaExtensionsCore.WebApi.ExtensionMethods;
using WebApiHypermediaExtensionsCore.WebApi.RouteResolver;

namespace WebApiHypermediaExtensionsCore.WebApi.AttributedRoutes
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