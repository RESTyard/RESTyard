using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using RESTyard.AspNetCore.WebApi.ExtensionMethods;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace RESTyard.AspNetCore.WebApi.AttributedRoutes;

// required because LinkGenerator changes with every HTTP context.
public class RegisterRouteResolverFactory : IRouteResolverFactory
{
    private readonly HypermediaExtensionsOptions hypermediaOptions;

    public RegisterRouteResolverFactory(
        HypermediaExtensionsOptions hypermediaOptions)
    {
        this.hypermediaOptions = hypermediaOptions;
    }

    public IHypermediaRouteResolver CreateRouteResolver(HttpContext httpContext, IHypermediaUrlConfig? urlConfig = null)
    {
        var linkGenerator = httpContext.RequestServices.GetRequiredService<LinkGenerator>();
        var routeRegister = httpContext.RequestServices.GetRequiredService<IRouteRegister>();
        var routeKeyFactory = httpContext.RequestServices.GetRequiredService<IRouteKeyFactory>();
        var hypermediaUrlConfig = urlConfig ?? HypermediaUrlConfigBuilder.Build(httpContext.Request);
        var routeResolver = new RegisterRouteResolver(httpContext, linkGenerator, routeKeyFactory, routeRegister, this.hypermediaOptions, hypermediaUrlConfig);
        return routeResolver;
    }
}