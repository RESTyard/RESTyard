using System;
using Bluehands.Hypermedia.Client.Resolver.Caching;

namespace Bluehands.Hypermedia.Client.Resolver
{
    public class NoLinkCache<TStrong, TWeak> : ILinkHcoCache<TStrong, TWeak>
    {
        private NoLinkCache()
        {

        }

        public static NoLinkCache<TStrong, TWeak> Instance { get; } = new NoLinkCache<TStrong, TWeak>();

        public bool TryGetValue(Uri uri, out CacheEntry<TStrong, TWeak> entry)
        {
            entry = default;
            return false;
        }

        public void Set(Uri uri, CacheEntry<TStrong, TWeak> entry)
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