using System;
using System.Collections.Generic;

namespace RESTyard.AspNetCore.WebApi.RouteResolver
{
    public class ResolvedRoute
    {
        public string Url { get; }

        public HttpMethod HttpMethod { get; }

        // for accessing resources (links) it indicates what media types are available
        public IReadOnlyCollection<string> AvailableMediaTypes { get; set; }
        
        //  for actions it defines what media type is acceptable 
        public string AcceptableMediaType { get; set; }

        public ResolvedRoute(string url, HttpMethod httpMethod, IReadOnlyCollection<string> availableMediaTypes = null, string acceptableMediaType = null)
        {
            Url = url;
            HttpMethod = httpMethod;
            AvailableMediaTypes = availableMediaTypes ?? Array.Empty<string>();
            AcceptableMediaType = acceptableMediaType;
        }
    }
}
