using System;
using Bluehands.Hypermedia.Client.Hypermedia;

namespace Bluehands.Hypermedia.Client.Resolver.Caching
{
    public class CacheEntry<TStrongValidator, TWeakValidator>
    {
        public HypermediaClientObject HypermediaClientObject { get; }

        public CacheMode CacheMode { get; }

        public CacheScope CacheScope { get; }

        public TStrongValidator StrongValidator { get; }

        public TWeakValidator WeakValidator { get; }

        public DateTime? LocalExpirationDate { get; }

        public CacheEntry(
            HypermediaClientObject hypermediaClientObject,
            CacheMode cacheMode,
            CacheScope cacheScope,
            TStrongValidator strongValidator,
            TWeakValidator weakValidator,
            DateTime? localExpirationDate)
        {
            HypermediaClientObject = hypermediaClientObject;
            CacheMode = cacheMode;
            CacheScope = cacheScope;
            StrongValidator = strongValidator;
            WeakValidator = weakValidator;
            LocalExpirationDate = localExpirationDate;
        }
    }
}