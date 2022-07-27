using System;
using Microsoft.AspNetCore.Http;

namespace RESTyard.AspNetCore.WebApi
{
    public interface IHypermediaUrlConfig
    {
        /// <summary>
        /// The Scheme which will be used when generating Hypermedia.
        /// </summary>
        string Scheme { get; }

        /// <summary>
        /// The Host which will be used when generating Hypermedia.
        /// </summary>
        HostString Host { get; }
    }
}