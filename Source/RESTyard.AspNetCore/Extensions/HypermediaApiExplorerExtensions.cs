using System.Collections.Generic;
using RESTyard.AspNetCore.Hypermedia;

namespace RESTyard.AspNetCore.Extensions;

public static class HypermediaApiExplorerExtensions
{
    public static IEnumerable<string> GetFullRouteTemplateFor<THto>(this IHypermediaApiExplorer apiExplorer)
        where THto : IHypermediaObject
        => apiExplorer.GetFullRouteTemplateFor(typeof(THto));
}