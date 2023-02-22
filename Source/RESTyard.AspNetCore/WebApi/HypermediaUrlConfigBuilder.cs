using System;
using Microsoft.AspNetCore.Http;

namespace RESTyard.AspNetCore.WebApi
{
    public static class HypermediaUrlConfigBuilder
    {
        public static IHypermediaUrlConfig Build(IHypermediaUrlConfig defaultHypermediaUrlConfig, HttpRequest request)
        {
            return new HypermediaUrlConfig()
            {
                Scheme = string.IsNullOrEmpty(defaultHypermediaUrlConfig.Scheme)
                    ? request.Scheme
                    : defaultHypermediaUrlConfig.Scheme,
                Host = defaultHypermediaUrlConfig.Host.HasValue ? defaultHypermediaUrlConfig.Host : request.Host,
            };
        }
    }
}