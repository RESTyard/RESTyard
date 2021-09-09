using System;
using Bluehands.Hypermedia.Client.Hypermedia;

namespace Bluehands.Hypermedia.Client.Resolver
{
    public interface ILinkHcoCache<TIdentifier>
    {
        bool TryGetValue(Uri uri, out CacheEntry<TIdentifier> entry);

        void Set(Uri uri, CacheEntry<TIdentifier> entry);

        void Remove(Uri uri);

        void Clear();

    }
    public readonly struct CacheEntry<TIdentifier>
    {
        public HypermediaClientObject HypermediaClientObject { get; }

        public TIdentifier Identifier { get; }

        public CacheEntry(HypermediaClientObject hypermediaClientObject, TIdentifier identifier)
        {
            HypermediaClientObject = hypermediaClientObject;
            Identifier = identifier;
        }
    }
}