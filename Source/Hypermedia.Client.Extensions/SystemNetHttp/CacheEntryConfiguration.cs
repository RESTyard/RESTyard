using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Bluehands.Hypermedia.Client.Resolver.Caching;

namespace Bluehands.Hypermedia.Client.Extensions.SystemNetHttp
{
    public class CacheEntryConfiguration
    {
        private CacheEntryConfiguration(bool hasCacheConfiguration)
        {
            HasCacheConfiguration = hasCacheConfiguration;
        }

        public CacheEntryConfiguration(
            CacheScope cacheScope,
            DateTimeOffset? localExpirationDate,
            CacheMode cacheMode,
            string etag,
            DateTimeOffset? lastModified)
            : this(hasCacheConfiguration: true)
        {
            CacheScope = cacheScope;
            LocalExpirationDate = localExpirationDate;
            CacheMode = cacheMode;
            Etag = etag;
            LastModified = lastModified;
        }

        public bool HasCacheConfiguration { get; }

        public CacheScope CacheScope { get; }

        public DateTimeOffset? LocalExpirationDate { get; }

        public CacheMode CacheMode { get; }

        public string Etag { get; }

        public string ETag { get; }

        public DateTimeOffset? LastModified { get; }

        public static CacheEntryConfiguration FromHttpResponse(
            HttpResponseMessage response,
            DateTimeOffset assumedNow)
        {
            var cc = response.Headers.CacheControl;
            if (cc is null)
            {
                return new CacheEntryConfiguration(false);
            }

            CacheMode mode = CacheMode.Undefined;
            CacheScope scope = CacheScope.Undefined;
            string etag = string.Empty;
            DateTimeOffset? lastModified = null;
            DateTimeOffset? expirationDate = null;

            if (cc.MustRevalidate)
            {
                mode = CacheMode.RevalidateStale;
            }
            if (cc.NoCache)
            {
                mode = CacheMode.AlwaysRevalidate;
            }
            if (cc.NoStore)
            {
                mode = CacheMode.DoNotCache;
            }

            if (cc.Public)
            {
                scope = CacheScope.AcrossUserContexts;
            }
            if (cc.Private)
            {
                scope = CacheScope.ForIndividualUserContext;
            }

            if (cc.MaxAge != null)
            {
                expirationDate = assumedNow + cc.MaxAge.Value;
            }
            if (!string.IsNullOrEmpty(response.Headers.ETag?.Tag))
            {
                etag = StringHelpers.RemoveSurroundingQuotes(response.Headers.ETag.Tag);
            }

            if (response.Content.Headers.LastModified != null)
            {
                lastModified = response.Content.Headers.LastModified.Value;
            }

            var hasImplicitCacheMode =
                scope != CacheScope.Undefined
                    || !string.IsNullOrEmpty(etag)
                    || lastModified != null
                    || expirationDate != null;
            if (hasImplicitCacheMode)
            {
                if (mode == CacheMode.Undefined)
                {
                    mode = CacheMode.NoRevalidationRequired;
                }
            }

            if (mode != CacheMode.Undefined)
            {
                return new CacheEntryConfiguration(
                    scope,
                    expirationDate,
                    mode,
                    etag,
                    lastModified);
            }

            return new CacheEntryConfiguration(false);
        }

        public static CacheEntryConfiguration EmptyConfiguration()
        {
            return new CacheEntryConfiguration(false);
        }

        public bool ShouldCache() => this.HasCacheConfiguration && this.CacheMode != CacheMode.DoNotCache;
    }
}