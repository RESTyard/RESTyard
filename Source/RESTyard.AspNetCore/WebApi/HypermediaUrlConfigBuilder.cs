using System;
using Microsoft.AspNetCore.Http;

namespace RESTyard.AspNetCore.WebApi
{
    public static class HypermediaUrlConfigBuilder
    {
        public static IHypermediaUrlConfig Build(HttpRequest request)
        {
            return new HypermediaUrlConfig()
            {
                Scheme = request.Scheme,
                Host = request.Host,
            };
        }
    }
}