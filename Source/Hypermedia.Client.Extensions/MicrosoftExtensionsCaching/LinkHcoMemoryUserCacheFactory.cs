using System;
using Bluehands.Hypermedia.Client.Resolver.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace MicrosoftExtensionsCaching
{
    public class LinkHcoMemoryUserCacheFactory<TUserIdentifier, TValidator>
    {
        private readonly IMemoryCache memoryCache;
        private readonly TUserIdentifier sharedUserIdentifier;
        private readonly Func<TUserIdentifier, Uri, object> hcoEntryKeyBuilder;
        private readonly Func<TUserIdentifier, object> controlEntryKeyBuilder;
        private readonly Action<ICacheEntry, LinkHcoCacheEntry<TValidator>> configureEntryExpiration;
        private readonly Action<ICacheEntry> configureControlExpiration;
        private readonly Action<ICacheEntry> configureRootExpiration;
        private readonly object rootControlTokenKey;

        public LinkHcoMemoryUserCacheFactory(
            IMemoryCache memoryCache,
            TUserIdentifier sharedUserIdentifier,
            Func<TUserIdentifier, Uri, object> hcoEntryKeyBuilder,
            Func<TUserIdentifier, object> controlEntryKeyBuilder,
            Action<ICacheEntry, LinkHcoCacheEntry<TValidator>> configureEntryExpiration,
            Action<ICacheEntry> configureControlExpiration,
            Action<ICacheEntry> configureRootExpiration,
            object rootControlTokenKey)
        {
            this.memoryCache = memoryCache;
            this.sharedUserIdentifier = sharedUserIdentifier;
            this.hcoEntryKeyBuilder = hcoEntryKeyBuilder;
            this.controlEntryKeyBuilder = controlEntryKeyBuilder;
            this.configureEntryExpiration = configureEntryExpiration;
            this.configureControlExpiration = configureControlExpiration;
            this.configureRootExpiration = configureRootExpiration;
            this.rootControlTokenKey = rootControlTokenKey;
        }

        public ILinkHcoCache<TValidator> CreateUserCache(TUserIdentifier currentUserIdentifier)
        {
            return new LinkHcoMemoryUserCache<TUserIdentifier, TValidator>(
                this.memoryCache,
                currentUserIdentifier,
                this.sharedUserIdentifier,
                this.hcoEntryKeyBuilder,
                this.controlEntryKeyBuilder,
                this.configureEntryExpiration,
                this.configureControlExpiration,
                this.configureRootExpiration,
                this.rootControlTokenKey);
        }
    }
}