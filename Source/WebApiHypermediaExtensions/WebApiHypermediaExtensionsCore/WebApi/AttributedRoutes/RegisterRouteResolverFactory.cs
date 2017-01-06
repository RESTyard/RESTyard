using Microsoft.AspNetCore.Mvc;
using WebApiHypermediaExtensionsCore.WebApi.RouteResolver;

namespace WebApiHypermediaExtensionsCore.WebApi.AttributedRoutes
{
    // required because theUrlHelper changes with every context.
    public class RegisterRouteResolverFactory : IRouteResolverFactory
    {
        private readonly IRouteRegister routeRegister;

        public RegisterRouteResolverFactory(IRouteRegister routeRegister)
        {
            this.routeRegister = routeRegister;
        }

        public IHypermediaRouteResolver CreateRouteResolver(IUrlHelper urlHelper, IRouteKeyFactory routeKeyFactory, HypermediaUrlConfig hypermediaUrlConfig = null)
        {
            return new RegisterRouteResolver(urlHelper, routeKeyFactory, routeRegister, hypermediaUrlConfig);
        }
    }


}