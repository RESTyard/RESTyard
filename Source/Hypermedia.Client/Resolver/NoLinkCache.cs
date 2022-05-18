using System;
using Bluehands.Hypermedia.Client.Resolver.Caching;

namespace Bluehands.Hypermedia.Client.Resolver
{
    public class NoLinkCache<TValidator> : ILinkHcoCache<TValidator>
    {
        private NoLinkCache()
        {

        }

        public static NoLinkCache<TValidator> Instance { get; } = new NoLinkCache<TValidator>();

        public bool TryGetValue(Uri uri, out CacheEntry<TValidator> entry)
        {
            entry = default;
            return false;
        }

        public void Set(Uri uri, CacheEntry<TValidator> entry)
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