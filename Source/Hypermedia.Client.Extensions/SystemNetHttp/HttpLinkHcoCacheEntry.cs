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

        public HttpLinkHcoCacheEntry(
            string linkResponseContent,
            CacheEntryConfiguration configuration)
            : this(
                linkResponseContent,
                configuration.CacheScope,
                configuration.LocalExpirationDate,
                configuration.CacheMode,
                configuration.ETag,
                configuration.LastModified)
        {
        }

        public CacheMode CacheMode { get; }

        public string Etag { get; }

        public string ETag { get; }

        public DateTimeOffset? LastModified { get; }

        public bool IsRevalidationRequired(DateTimeOffset assumedNow)
        {
            var isStale = this.LocalExpirationDate == null || this.LocalExpirationDate < assumedNow;
            bool mustRevalidate =
                this.CacheMode == CacheMode.AlwaysRevalidate
                || (isStale && this.CacheMode == CacheMode.RevalidateStale);
            return mustRevalidate;
        }
    }
}