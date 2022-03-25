using System;
using System.Net.Http;
using Bluehands.Hypermedia.Client.Resolver;

namespace Bluehands.Hypermedia.Client.Extensions.SystemNetHttp
{
    public static class SystemNetHttpExtensions
    {
        /// <summary>
        /// Use to resolve URIs over HTTP
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="httpClient">A HttpClient to be used by the resolver. The Client will be disposed when the resolver is disposed</param>
        /// <param name="configure">Additional configuration for the resolver, such as setting credentials or custom default headers</param>
        /// <returns></returns>
        public static HypermediaClientBuilder WithHttpHypermediaResolver(
            this HypermediaClientBuilder builder,
            HttpClient httpClient,
            Action<IHttpHypermediaResolverConfiguration> configure)
        {
            return builder.WithCustomHypermediaResolver((serializer, problemReader) =>
            {
                var resolver = new HttpHypermediaResolver(
                    httpClient,
                    serializer,
                    problemReader,
                    NoLinkCache<string>.Instance);
                configure(resolver);
                return resolver;
            });
        }

        /// <summary>
        /// Use to resolve URIs over HTTP. Caches incoming links that have an ETag. Additional code is needed from the server to make use of caching. See https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/ETag
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="httpClient">A HttpClient to be used by the resolver. The Client will be disposed when the resolver is disposed</param>
        /// <param name="linkHcoCache">The cache to store incoming links in. The cache is responsible to adhere to capacity limits and implement replacement strategies</param>
        /// <param name="configure">Additional configuration for the resolver, such as setting credentials or custom default headers</param>
        /// <returns></returns>
        public static HypermediaClientBuilder WithCachedHttpHypermediaResolver(
            this HypermediaClientBuilder builder,
            HttpClient httpClient,
            ILinkHcoCache<string> linkHcoCache,
            Action<IHttpHypermediaResolverConfiguration> configure)
        {
            return builder.WithCustomHypermediaResolver((serializer, problemReader) =>
            {
                var resolver = new HttpHypermediaResolver(
                    httpClient,
                    serializer,
                    problemReader,
                    linkHcoCache);
                configure(resolver);
                return resolver;
            });
        }
    }
}
