using System;

namespace RESTyard.Client.Resolver.Caching
{
    public interface ILinkHcoCache<TLinkHcoCacheEntry>
        where TLinkHcoCacheEntry : LinkHcoCacheEntry
    {
        bool TryGetValue(Uri uri, out TLinkHcoCacheEntry entry);

        void Set(Uri uri, TLinkHcoCacheEntry entry);

        void Replace(
            Uri uri,
            TLinkHcoCacheEntry oldEntry,
            TLinkHcoCacheEntry newEntry);

        void Remove(Uri uri);

        void Clear();
    }
}