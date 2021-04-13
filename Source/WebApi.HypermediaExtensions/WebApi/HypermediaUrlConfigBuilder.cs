namespace WebApi.HypermediaExtensions.WebApi
{
    using Microsoft.AspNetCore.Http;

    public class HypermediaUrlConfigBuilder : IHypermediaUrlConfig
    {
        public HypermediaUrlConfigBuilder(IHypermediaUrlConfig defaultHypermediaUrlConfig, HttpRequest request)
        {
            this.Scheme = string.IsNullOrEmpty(defaultHypermediaUrlConfig.Scheme) ? request.Scheme : defaultHypermediaUrlConfig.Scheme;
            this.Host = defaultHypermediaUrlConfig.Host.HasValue ? defaultHypermediaUrlConfig.Host : request.Host;
        }

        public string Scheme { get; }

        public HostString Host { get; }
    }
}