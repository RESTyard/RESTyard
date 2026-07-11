using System;
using System.Collections.Generic;
using FunicularSwitch;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.AspNetCore.Hypermedia.Links;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace RESTyard.HtoSourceGenerators.Test;

/// <summary>
/// Configurable stub for <see cref="IHypermediaRouteResolver"/> used in parity tests.
/// Routes are registered via <see cref="RouteMapping"/> entries — deterministic URLs that feed
/// both the generated <c>ToSiren()</c> and the reflection-based <c>SirenConverter</c>.
/// </summary>
internal class StubRouteResolver : IHypermediaRouteResolver
{
    private readonly Dictionary<Type, ResolvedRoute> objectRoutes = new();
    private readonly Dictionary<(Type, Type), ResolvedRoute> actionRoutes = new();
    private readonly Dictionary<Type, ResolvedRoute> typeRoutes = new();
    private readonly ResolvedRoute fallbackRoute;

    public StubRouteResolver(ResolvedRoute? fallbackRoute = null, params RouteMapping[] mappings)
    {
        this.fallbackRoute = fallbackRoute ?? new ResolvedRoute("http://test/fallback", "GET");

        foreach (var mapping in mappings)
        {
            switch (mapping.Kind)
            {
                case RouteMappingKind.Object:
                    objectRoutes[mapping.HtoType] = mapping.Route;
                    break;
                case RouteMappingKind.Action:
                    actionRoutes[(mapping.HtoType, mapping.ActionType!)] = mapping.Route;
                    break;
                case RouteMappingKind.Type:
                    typeRoutes[mapping.HtoType] = mapping.Route;
                    break;
            }
        }
    }

    public ResolvedRoute ObjectToRoute(IHypermediaObject hypermediaObject)
        => objectRoutes.GetValueOrDefault(hypermediaObject.GetType()) ?? fallbackRoute;

    public ResolvedRoute ReferenceToRoute(HypermediaObjectReferenceBase reference)
    {
        // Mirror RegisterRouteResolver: ExternalReference carries its own URI and media types
        if (reference.GetInstance() is ExternalReference externalReference)
        {
            return new ResolvedRoute(
                externalReference.ExternalUri.ToString(), "GET", externalReference.AvailableMediaTypes);
        }

        var htoType = reference.GetHypermediaType();
        return objectRoutes.GetValueOrDefault(htoType) ?? fallbackRoute;
    }

    public ResolvedRoute ActionToRoute(IHypermediaObject hypermediaObject, HypermediaActionBase reference)
    {
        var htoType = hypermediaObject.GetType();
        var actionType = reference.GetType();
        return actionRoutes.GetValueOrDefault((htoType, actionType)) ?? fallbackRoute;
    }

    public ResolvedRoute TypeToRoute(Type actionParameterType)
        => typeRoutes.GetValueOrDefault(actionParameterType) ?? fallbackRoute;

    public Option<ResolvedRoute> TryGetRouteByType(Type type, object? routeKeys = null)
    {
        var route = typeRoutes.GetValueOrDefault(type);
        return route != null ? Option.Some(route) : Option<ResolvedRoute>.None;
    }

    public Result<string> RouteUrl(string routeName, object? routeKeys = null)
        => $"http://test/routes/{routeName}";

    public Result<ResolvedRoute> RouteUrl(RouteInfo routeInfo, object? routeKeys = null)
        => new ResolvedRoute($"http://test/routes/{routeInfo}", "GET");
}

internal enum RouteMappingKind { Object, Action, Type }

internal record RouteMapping(RouteMappingKind Kind, Type HtoType, ResolvedRoute Route, Type? ActionType = null)
{
    public static RouteMapping ForObject<THto>(string url, string httpMethod = "GET")
        => new(RouteMappingKind.Object, typeof(THto), new ResolvedRoute(url, httpMethod));

    public static RouteMapping ForAction<THto, TAction>(string url, string httpMethod = "POST")
        where TAction : HypermediaActionBase
        => new(RouteMappingKind.Action, typeof(THto), new ResolvedRoute(url, httpMethod), typeof(TAction));

    public static RouteMapping ForType<T>(string url)
        => new(RouteMappingKind.Type, typeof(T), new ResolvedRoute(url, null));
}
