using System;
using Bluehands.Hypermedia.Client.Hypermedia;

namespace Bluehands.Hypermedia.Client.Resolver.Caching
{
    public class CacheEntry<TValidator>
    {
        public HypermediaClientObject HypermediaClientObject { get; }

        public CacheMode CacheMode { get; }

        public CacheScope CacheScope { get; }

        public TValidator Validator { get; }

        public DateTime? LocalExpirationDate { get; }

        public CacheEntry(
            HypermediaClientObject hypermediaClientObject,
            CacheMode cacheMode,
            CacheScope cacheScope,
            TValidator validator,
            DateTime? localExpirationDate)
        {
            HypermediaClientObject = hypermediaClientObject;
            CacheMode = cacheMode;
            CacheScope = cacheScope;
            Validator = validator;
            LocalExpirationDate = localExpirationDate;
        }
    }
}