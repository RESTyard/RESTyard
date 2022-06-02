using System;

namespace Bluehands.Hypermedia.Client.Resolver.Caching
{
    public class LinkHcoCacheEntry
    {
        public LinkHcoCacheEntry(
            string linkResponseContent,
            CacheScope cacheScope,
            DateTimeOffset? localExpirationDate)
        {
            LinkResponseContent = linkResponseContent;
            CacheScope = cacheScope;
            LocalExpirationDate = localExpirationDate;
        }

        public string LinkResponseContent { get; }

        public CacheScope CacheScope { get; }

        public DateTimeOffset? LocalExpirationDate { get; }
    }
}