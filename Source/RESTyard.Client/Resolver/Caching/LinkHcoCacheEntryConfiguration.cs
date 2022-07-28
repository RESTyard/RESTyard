using System;

namespace RESTyard.Client.Resolver.Caching
{
    public class LinkHcoCacheEntryConfiguration
    {
        protected LinkHcoCacheEntryConfiguration(bool hasConfiguration)
        {
            HasCacheConfiguration = hasConfiguration;
        }

        public LinkHcoCacheEntryConfiguration(
            CacheScope cacheScope,
            DateTimeOffset? localExpirationDate)
            : this(hasConfiguration: true)
        {
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