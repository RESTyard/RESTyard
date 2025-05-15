using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace RESTyard.AspNetCore.Test.WebApi.Formatter;

public class FakeLinkGenerator : LinkGenerator
{
    public override string? GetPathByAddress<TAddress>(HttpContext httpContext, TAddress address, RouteValueDictionary values,
        RouteValueDictionary? ambientValues = null, PathString? pathBase = null,
        FragmentString fragment = new FragmentString(), LinkOptions? options = null)
    {
        throw new NotImplementedException();
    }

    public override string? GetPathByAddress<TAddress>(TAddress address, RouteValueDictionary values,
        PathString pathBase = new PathString(), FragmentString fragment = new FragmentString(),
        LinkOptions? options = null)
    {
        throw new NotImplementedException();
    }

    public override string? GetUriByAddress<TAddress>(HttpContext httpContext, TAddress address, RouteValueDictionary values,
        RouteValueDictionary? ambientValues = null, string? scheme = null, HostString? host = null,
        PathString? pathBase = null, FragmentString fragment = new FragmentString(), LinkOptions? options = null)
    {
        var result = $"{scheme}://{host}/{address}";
        foreach (var kvp in values)
        {
            result += $"/{{ {kvp.Key} = {kvp.Value} }}";
        }
        return result;
    }

    public override string? GetUriByAddress<TAddress>(TAddress address, RouteValueDictionary values, string? scheme, HostString host,
        PathString pathBase = new PathString(), FragmentString fragment = new FragmentString(),
        LinkOptions? options = null)
    {
        throw new NotImplementedException();
    }
}