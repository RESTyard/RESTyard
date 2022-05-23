using System;

namespace Bluehands.Hypermedia.Client.Resolver.Caching
{
    public interface ILinkHcoCache<TLinkHcoCacheEntry>
        where TLinkHcoCacheEntry : LinkHcoCacheEntry
    {
        bool TryGetValue(Uri uri, out TLinkHcoCacheEntry entry);

        void Set(Uri uri, TLinkHcoCacheEntry entry);

        void Remove(Uri uri);

        void Clear();
    }
}