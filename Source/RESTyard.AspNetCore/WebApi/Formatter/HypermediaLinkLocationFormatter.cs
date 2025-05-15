using Microsoft.AspNetCore.Http;
using RESTyard.AspNetCore.Hypermedia.Links;
using RESTyard.AspNetCore.Query;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace RESTyard.AspNetCore.WebApi.Formatter;

public class HypermediaLinkLocationFormatter : HypermediaLocationFormatter<HypermediaLinkLocation>
{
    private readonly IQueryStringBuilder queryStringBuilder;

    public HypermediaLinkLocationFormatter(
        IRouteResolverFactory routeResolverFactory,
        IQueryStringBuilder queryStringBuilder)
        : base(routeResolverFactory)
    {
        this.queryStringBuilder = queryStringBuilder;
    }

    protected override void SetResponseValues(HttpResponse response, HypermediaLinkLocation item)
    {
        response.StatusCode = (int)item.HttpStatusCode;
    }

    protected override string GetLocation(IHypermediaRouteResolver routeResolver, HypermediaLinkLocation item)
    {
        var reference = item.Link.Reference;
        var route = routeResolver.ReferenceToRoute(reference);
        var query = reference.GetQuery();
        if (query is not null)
        {
            var queryString = queryStringBuilder.CreateQueryString(query);
            if (!string.IsNullOrEmpty(queryString))
            {
                return route.Url + queryString;
            }
        }

        return route.Url;
    }

    protected override HypermediaLinkLocation? GetObject(object? locationObject)
    {
        return locationObject as HypermediaLinkLocation;
    }
}