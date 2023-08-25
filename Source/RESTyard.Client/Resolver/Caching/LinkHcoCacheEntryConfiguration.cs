using System;

namespace RESTyard.Client.Resolver.Caching
{
    public class LinkHcoCacheEntryConfiguration
    {
        public LinkHcoCacheEntryConfiguration(
            bool hasConfiguration,
            CacheScope cacheScope,
            DateTimeOffset? localExpirationDate = null)
        {
            HasCacheConfiguration = hasConfiguration;
            CacheScope = cacheScope;
            LocalExpirationDate = localExpirationDate;
        }

        public bool HasCacheConfiguration { get; }

        public CacheScope CacheScope { get; }

        public DateTimeOffset? LocalExpirationDate { get; }

        public virtual bool ShouldBeAddedToCache()
        {
            return this.HasCacheConfiguration;
        }
    }
}