using System;

namespace Bluehands.Hypermedia.Client.Resolver.Caching
{
    public interface ILinkHcoCache<TStrongValidator, TWeakValidator>
    {
        bool TryGetValue(Uri uri, out CacheEntry<TStrongValidator, TWeakValidator> entry);

        void Set(Uri uri, CacheEntry<TStrongValidator, TWeakValidator> entry);

        void Remove(Uri uri);

        void Clear();
    }
}