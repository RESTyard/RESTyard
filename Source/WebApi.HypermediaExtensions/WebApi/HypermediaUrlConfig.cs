using System;
using Microsoft.AspNetCore.Http;

namespace RESTyard.WebApi.Extensions.WebApi
{
    /// <summary>
    /// Configures the URL building for generation of in Hypermedia documents. 
    /// </summary>
    public class HypermediaUrlConfig : IHypermediaUrlConfig
    {
        public string Scheme { get; set; }

        public HostString Host { get; set; }
    }
}