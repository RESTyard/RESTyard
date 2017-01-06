using Microsoft.AspNetCore.Http;

namespace WebApiHypermediaExtensionsCore.WebApi
{
    /// <summary>
    /// Configures the URL building for generation of in Hypermedia documents. 
    /// </summary>
    public class HypermediaUrlConfig
    {
        public HypermediaUrlConfig()
        {
        }

        public HypermediaUrlConfig(HypermediaUrlConfig defaultHypermediaUrlConfig, HttpRequest request)
        {
            Scheme = string.IsNullOrEmpty(defaultHypermediaUrlConfig.Scheme) ? request.Scheme    : defaultHypermediaUrlConfig.Scheme;
            Host   = defaultHypermediaUrlConfig.Host.HasValue ? defaultHypermediaUrlConfig.Host : request.Host;
        }

        /// <summary>
        /// The Scheme which will be used when generating Hypermedia.
        /// </summary>
        public string Scheme { get; set; }

        /// <summary>
        /// The Host which will be used when generating Hypermedia.
        /// </summary>
        public HostString Host { get; set; }
    }
}