using Microsoft.AspNetCore.Http;

namespace WebApiHypermediaExtensionsCore.WebApi
{
    public interface IHypermediaUrlConfig
    {
        /// <summary>
        /// The Scheme which will be used when generating Hypermedia.
        /// </summary>
        string Scheme { get; set; }

        /// <summary>
        /// The Host which will be used when generating Hypermedia.
        /// </summary>
        HostString Host { get; set; }
    }
}