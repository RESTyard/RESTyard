using System;
using System.Net.Http;
using RESTyard.Client.Resolver.Caching;

namespace RESTyard.Client.Extensions.SystemNetHttp
{
    public class HttpLinkHcoCacheEntryConfiguration : LinkHcoCacheEntryConfiguration
    {
        public static HttpLinkHcoCacheEntryConfiguration NoConfiguration() => new(false, CacheScope.Undefined, null,
            CacheMode.Undefined, string.Empty, null);

        public HttpLinkHcoCacheEntryConfiguration(
            bool hasConfiguration,
            CacheScope cacheScope,
            DateTimeOffset? localExpirationDate,
            CacheMode cacheMode,
            string etag,
            DateTimeOffset? lastModified)
            : base(hasConfiguration, cacheScope, localExpirationDate)
        {
            CacheMode = cacheMode;
            ETag = etag;
            LastModified = lastModified;
        }

        public CacheMode CacheMode { get; }

        public string ETag { get; }

        public DateTimeOffset? LastModified { get; }

        public static HttpLinkHcoCacheEntryConfiguration FromHttpResponse(
            HttpResponseMessage response,
            DateTimeOffset assumedNow)
        {
            var cc = response.Headers.CacheControl;
            if (cc is null)
            {
                return NoConfiguration();
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
                return new HttpLinkHcoCacheEntryConfiguration(
                    true,
                    scope,
                    expirationDate,
                    mode,
                    etag,
                    lastModified);
            }

            return NoConfiguration();
        }

        public override bool ShouldBeAddedToCache() => base.ShouldBeAddedToCache() && this.CacheMode != CacheMode.DoNotCache;
    }
}