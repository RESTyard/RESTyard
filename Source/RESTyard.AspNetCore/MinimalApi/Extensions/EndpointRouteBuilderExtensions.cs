using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.WebApi.RouteResolver;
using RESTyard.MediaTypes;

namespace RESTyard.AspNetCore.MinimalApi.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointConventionBuilder MapGetHypermediaObject<THypermediaObject>(
        this IEndpointRouteBuilder endpoints,
        [StringSyntax("Route")] string pattern,
        System.Delegate handler)
        where THypermediaObject : HypermediaObject
    {
        var routeRegister = endpoints.ServiceProvider.GetRequiredService<IRouteRegister>();
        var endpointName = "RESTyard_HypermediaEndpoint_" + (typeof(THypermediaObject).FullName ?? "HypermediaObject_" + typeof(THypermediaObject).Name);
        routeRegister.AddHypermediaObjectRoute(typeof(THypermediaObject), endpointName, HttpMethod.GET);
        return endpoints
            .MapGet(pattern, handler)
            .WithName(endpointName)
            .Produces<THypermediaObject>(200, DefaultMediaTypes.Siren);
    }

    public static IEndpointConventionBuilder MapGetHypermediaObject<THypermediaObject>(
        this IEndpointRouteBuilder endpoints,
        [StringSyntax("Route")] string pattern,
        RequestDelegate requestDelegate)
        where THypermediaObject : HypermediaObject
    {
        var routeRegister = endpoints.ServiceProvider.GetRequiredService<IRouteRegister>();
        var endpointName = "RESTyard_HypermediaEndpoint_" + (typeof(THypermediaObject).FullName ?? "HypermediaObject_" + typeof(THypermediaObject).Name);
        routeRegister.AddHypermediaObjectRoute(typeof(THypermediaObject), endpointName, HttpMethod.GET);
        return endpoints
            .MapGet(pattern, requestDelegate)
            .WithName(endpointName);
    }
}