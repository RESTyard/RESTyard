using System;

namespace Bluehands.Hypermedia.Client.Resolver.Caching
{
    public interface ILinkHcoCache<TValidator>
    {
        bool TryGetValue(Uri uri, out CacheEntry<TValidator> entry);

        void Set(Uri uri, CacheEntry<TValidator> entry);

        void Remove(Uri uri);

        void Clear();
    }
}