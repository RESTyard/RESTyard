using System;
using System.Diagnostics.CodeAnalysis;
using RESTyard.Client.Resolver.Caching;

namespace RESTyard.Client.Resolver
{
    public class NoLinkCache<TLinkHcoCacheEntry> : ILinkHcoCache<TLinkHcoCacheEntry>
        where TLinkHcoCacheEntry : LinkHcoCacheEntry
    {
        private NoLinkCache()
        {
        }

        public static NoLinkCache<TLinkHcoCacheEntry> Instance { get; } = new NoLinkCache<TLinkHcoCacheEntry>();

        public bool TryGetValue(Uri uri, [NotNullWhen(true)] out TLinkHcoCacheEntry? entry)
        {
            entry = default;
            return false;
        }

        public void Set(Uri uri, TLinkHcoCacheEntry entry)
        {
        }

        public void Replace(
            Uri uri,
            TLinkHcoCacheEntry oldEntry,
            TLinkHcoCacheEntry newEntry)
        {
        }

        public void Remove(Uri uri)
        {
        }

        public void Clear()
        {
        }
    }
}