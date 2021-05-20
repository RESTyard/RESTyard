using System;
using Bluehands.Hypermedia.Client;
using Bluehands.Hypermedia.Client.Extensions.SystemNetHttp;
using Bluehands.Hypermedia.Client.Reader;
using Bluehands.Hypermedia.Client.Resolver;

namespace Bluehands.Hypermedia.Client.Extensions.SystemNetHttp
{
    public static class SystemNetHttpExtensions
    {
        public static HypermediaClientBuilder WithHttpHypermediaResolver(
            this HypermediaClientBuilder builder,
            Action<HttpHypermediaResolver> configure)
        {
            return builder.WithCustomHypermediaResolver((serializer, problemReader) =>
            {
                var resolver = new HttpHypermediaResolver(serializer, problemReader, NoLinkCache<string>.Instance);
                configure(resolver);
                return resolver;
            });
        }

        public static HypermediaClientBuilder WithCachedHttpHypermediaResolver(
            this HypermediaClientBuilder builder,
            ILinkHcoCache<string> linkHcoCache,
            Action<HttpHypermediaResolver> configure)
        {
            return builder.WithCustomHypermediaResolver((serializer, problemReader) =>
            {
                var resolver = new HttpHypermediaResolver(serializer, problemReader, linkHcoCache);
                configure(resolver);
                return resolver;
            });
        }
    }
}
