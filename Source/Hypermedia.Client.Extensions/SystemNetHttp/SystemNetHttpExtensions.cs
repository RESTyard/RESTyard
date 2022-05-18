using System;
using Bluehands.Hypermedia.Client.Resolver;
using Bluehands.Hypermedia.Client.Resolver.Caching;

namespace Bluehands.Hypermedia.Client.Extensions.SystemNetHttp
{
    public static class SystemNetHttpExtensions
    {
        /// <summary>
        /// Use to resolve URIs over HTTP
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure">Additional configuration for the resolver, such as setting credentials or custom default headers</param>
        /// <returns></returns>
        public static HypermediaClientBuilder WithHttpHypermediaResolver(
            this HypermediaClientBuilder builder,
            Action<IHttpHypermediaResolverConfiguration> configure)
        {
            return builder.WithCustomHypermediaResolver((serializer, problemReader) =>
            {
                var resolver = new HttpHypermediaResolver(
                    serializer,
                    problemReader,
                    NoLinkCache<HttpResponseValidator>.Instance);
                configure(resolver);
                return resolver;
            });
        }

        /// <summary>
        /// Use to resolve URIs over HTTP. Caches incoming links that have an ETag. Additional code is needed from the server to make use of caching. See https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/ETag
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="linkHcoCache">The cache to store incoming links in. The cache is responsible to adhere to capacity limits and implement replacement strategies</param>
        /// <param name="configure">Additional configuration for the resolver, such as setting credentials or custom default headers</param>
        /// <returns></returns>
        public static HypermediaClientBuilder WithCachedHttpHypermediaResolver(
            this HypermediaClientBuilder builder,
            ILinkHcoCache<HttpResponseValidator> linkHcoCache,
            Action<IHttpHypermediaResolverConfiguration> configure)
        {
            return builder.WithCustomHypermediaResolver((serializer, problemReader) =>
            {
                var resolver = new HttpHypermediaResolver(
                    serializer,
                    problemReader,
                    linkHcoCache);
                configure(resolver);
                return resolver;
            });
        }
    }
}
