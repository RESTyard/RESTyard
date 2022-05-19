using System;

namespace Bluehands.Hypermedia.Client.Resolver.Caching
{
    public interface ILinkHcoCache<TValidator>
    {
        bool TryGetValue(Uri uri, out LinkHcoCacheEntry<TValidator> entry);

        void Set(Uri uri, LinkHcoCacheEntry<TValidator> entry);

        void Remove(Uri uri);

        void Clear();
    }
}