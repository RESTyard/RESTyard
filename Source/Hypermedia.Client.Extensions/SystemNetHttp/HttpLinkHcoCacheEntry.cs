using System;
using Bluehands.Hypermedia.Client.Resolver.Caching;

namespace Bluehands.Hypermedia.Client.Extensions.SystemNetHttp
{
    public class HttpLinkHcoCacheEntry : LinkHcoCacheEntry
    {
        public HttpLinkHcoCacheEntry(
            string linkResponseContent,
            CacheScope cacheScope,
            DateTimeOffset? localExpirationDate,
            CacheMode cacheMode,
            string etag,
            DateTimeOffset? lastModified)
            : base(linkResponseContent, cacheScope, localExpirationDate)
        {
            CacheMode = cacheMode;
            Etag = etag;
            LastModified = lastModified;
        }

        public CacheMode CacheMode { get; }

        public string Etag { get; }

        public string ETag { get; }

        public DateTimeOffset? LastModified { get; }

        public static HttpLinkHcoCacheEntry FromConfiguration(
            string linkResponseContent,
            CacheEntryConfiguration configuration)
        {
            return new HttpLinkHcoCacheEntry(
                linkResponseContent,
                configuration.CacheScope,
                configuration.LocalExpirationDate,
                configuration.CacheMode,
                configuration.ETag,
                configuration.LastModified);
        }

        public bool IsRevalidationRequired(DateTimeOffset assumedNow)
        {
            var isStale = this.LocalExpirationDate == null || this.LocalExpirationDate < assumedNow;
            bool mustRevalidate =
                this.CacheMode == CacheMode.AlwaysRevalidate
                || (isStale && this.CacheMode == CacheMode.RevalidateStale);
            return mustRevalidate;
        }

        public bool IsConfigurationEquivalentTo(CacheEntryConfiguration configuration)
        {
            return this.CacheMode == configuration.CacheMode
               && this.CacheScope == configuration.CacheScope
               && this.LocalExpirationDate == configuration.LocalExpirationDate
               && this.ETag == configuration.ETag
               && this.LastModified == configuration.LastModified;
        }
    }
}