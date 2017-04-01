using Microsoft.AspNetCore.Http;

namespace WebApiHypermediaExtensionsCore.WebApi
{
    /// <summary>
    /// Configures the URL building for generation of in Hypermedia documents. 
    /// </summary>
    public class HypermediaUrlConfig : IHypermediaUrlConfig
    {
        public HypermediaUrlConfig()
        {
        }

        public HypermediaUrlConfig(IHypermediaUrlConfig defaultHypermediaUrlConfig, HttpRequest request)
        {
            Scheme = string.IsNullOrEmpty(defaultHypermediaUrlConfig.Scheme) ? request.Scheme   : defaultHypermediaUrlConfig.Scheme;
            Host   = defaultHypermediaUrlConfig.Host.HasValue ? defaultHypermediaUrlConfig.Host : request.Host;
        }

        public string Scheme { get; set; }

        public HostString Host { get; set; }
    }
}