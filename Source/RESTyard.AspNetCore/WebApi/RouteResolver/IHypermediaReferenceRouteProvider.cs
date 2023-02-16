using Microsoft.AspNetCore.Http;
using RESTyard.AspNetCore.Hypermedia.Links;

namespace RESTyard.AspNetCore.WebApi.RouteResolver;

public interface IHypermediaReferenceRouteProvider
{
    string GetRouteUri(HttpContext context, HypermediaObjectReferenceBase referenceBase);
}