using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using RESTyard.AspNetCore.WebApi.ExtensionMethods;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace RESTyard.AspNetCore.WebApi.AttributedRoutes;

// required because LinkGenerator changes with every HTTP context.
public class RegisterRouteResolverFactory : IRouteResolverFactory
{
    private readonly IRouteRegister routeRegister;
    private readonly HypermediaExtensionsOptions hypermediaOptions;
    private readonly IRouteKeyFactory routeKeyFactory;

    public RegisterRouteResolverFactory(
        IRouteRegister routeRegister,
        HypermediaExtensionsOptions hypermediaOptions,
        IRouteKeyFactory routeKeyFactory)
    {
        this.routeRegister = routeRegister;
        this.hypermediaOptions = hypermediaOptions;
        this.routeKeyFactory = routeKeyFactory;
    }

    public IHypermediaRouteResolver CreateRouteResolver(HttpContext httpContext, IHypermediaUrlConfig? urlConfig = null)
    {
        var linkGenerator = httpContext.RequestServices.GetRequiredService<LinkGenerator>();
        var hypermediaUrlConfig = urlConfig ?? HypermediaUrlConfigBuilder.Build(httpContext.Request);
        var routeResolver = new RegisterRouteResolver(httpContext, linkGenerator, this.routeKeyFactory, this.routeRegister, this.hypermediaOptions, hypermediaUrlConfig);
        return routeResolver;
    }
}