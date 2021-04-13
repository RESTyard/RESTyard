namespace WebApi.HypermediaExtensions.WebApi
{
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Configures the URL building for generation of in Hypermedia documents. 
    /// </summary>
    public class HypermediaUrlConfig : IHypermediaUrlConfig
    {
        public string Scheme { get; set; }

        public HostString Host { get; set; }
    }
}