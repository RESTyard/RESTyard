using System;

namespace Bluehands.Hypermedia.Client.Resolver
{
    public class NoLinkCache<T> : ILinkHcoCache<T>
    {
        private NoLinkCache()
        {

        }

        public static NoLinkCache<T> Instance { get; } = new NoLinkCache<T>();

        public bool TryGetValue(Uri uri, out CacheEntry<T> entry)
        {
            entry = new CacheEntry<T>();
            return false;
        }

        public void Set(Uri uri, CacheEntry<T> entry)
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