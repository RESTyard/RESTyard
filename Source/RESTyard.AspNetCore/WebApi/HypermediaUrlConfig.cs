using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;

namespace RESTyard.AspNetCore.WebApi
{
    /// <summary>
    /// Configures the URL building for generation of in Hypermedia documents. 
    /// </summary>
    public class HypermediaUrlConfig : IHypermediaUrlConfig
    {
        [DisallowNull]
        public string? Scheme { get; set; }

        public HostString Host { get; set; }
    }
}