using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using RESTyard.AspNetCore.WebApi.AttributedRoutes;

namespace RESTyard.AspNetCore;

public class HypermediaApiExplorer(IApiDescriptionGroupCollectionProvider apiExplorer) : IHypermediaApiExplorer
{
    private readonly IApiDescriptionGroupCollectionProvider apiExplorer = apiExplorer;
    
    public IReadOnlyCollection<string> GetFullRouteTemplateFor(Type type)
    {
        var result = this.apiExplorer.ApiDescriptionGroups.Items
            .SelectMany(i => i.Items)
            .Where(a => a.ActionDescriptor.EndpointMetadata
                .Any(m => (m is IHypermediaObjectEndpointMetadata hoem && type.IsAssignableFrom(hoem.RouteType))))
            .Select(a => a.RelativePath!)
            .ToImmutableList();
        return result;
    }

    public IReadOnlyCollection<ApiDescription> GetHypermediaEndpoints()
    {
        var result = this.apiExplorer.ApiDescriptionGroups.Items
            .SelectMany(i => i.Items)
            .Where(a => a.ActionDescriptor.EndpointMetadata
                .Any(m => m is IHypermediaEndpointMetadata))
            .ToImmutableList();
        return result;
    }
}