using System;

namespace Bluehands.Hypermedia.Client.Resolver.Caching
{
    public class LinkHcoCacheEntry<TValidator>
    {
        public string LinkResponseContent { get; set; }

        public CacheMode CacheMode { get; set; }

        public CacheScope CacheScope { get; set; }

        public TValidator Validator { get; set; }

        public DateTimeOffset? LocalExpirationDate { get; set; }

        public LinkHcoCacheEntry(
            string linkResponseContent,
            CacheMode cacheMode,
            CacheScope cacheScope,
            TValidator validator,
            DateTimeOffset? localExpirationDate)
        {
            LinkResponseContent = linkResponseContent;
            CacheMode = cacheMode;
            CacheScope = cacheScope;
            Validator = validator;
            LocalExpirationDate = localExpirationDate;
        }
    }
}