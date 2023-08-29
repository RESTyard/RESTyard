using System;
using System.Diagnostics.CodeAnalysis;

namespace RESTyard.Client.Resolver.Caching
{
    public interface ILinkHcoCache<TLinkHcoCacheEntry>
        where TLinkHcoCacheEntry : LinkHcoCacheEntry
    {
        bool TryGetValue(Uri uri, [NotNullWhen(true)] out TLinkHcoCacheEntry? entry);

        void Set(Uri uri, TLinkHcoCacheEntry entry);

        void Replace(
            Uri uri,
            TLinkHcoCacheEntry oldEntry,
            TLinkHcoCacheEntry newEntry);

        void Remove(Uri uri);

        void Clear();
    }
}