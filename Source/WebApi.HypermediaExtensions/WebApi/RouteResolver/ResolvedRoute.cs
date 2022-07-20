using System;
using System.Collections.Generic;

namespace WebApi.HypermediaExtensions.WebApi.RouteResolver
{
    public class ResolvedRoute
    {
        public string Url { get; }

        public HttpMethod HttpMethod { get; }

        public IReadOnlyCollection<string> AvailableMediaTypes { get; set; }

        public ResolvedRoute(string url, HttpMethod httpMethod, IReadOnlyCollection<string> availableMediaTypes = null)
        {
            Url = url;
            HttpMethod = httpMethod;
            AvailableMediaTypes = availableMediaTypes ?? Array.Empty<string>();
        }
    }
}
