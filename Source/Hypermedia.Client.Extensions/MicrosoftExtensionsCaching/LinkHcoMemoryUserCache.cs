using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Bluehands.Hypermedia.Client.Resolver.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace MicrosoftExtensionsCaching
{
    public static class LinkHcoMemoryUserCache
    {
        public static void ClearAllLinkHcoCacheEntries(
            IMemoryCache memoryCache,
            object rootControlTokenKey)
        {
            memoryCache.Remove(rootControlTokenKey);
        }

        public static Action<ICacheEntry, LinkHcoCacheEntry<TValidator>> DefaultEntryExpirationConfiguration<TValidator>()
        {
            return (iEntry, entry) =>
            {
                iEntry.Size = (entry.LinkResponseContent.Length * sizeof(char)) + sizeof(int);
            };
        }

        public static Action<ICacheEntry> DefaultControlExpirationConfiguration { get; } = entry =>
        {
            entry.Size = 0;
        };

        public static Action<ICacheEntry> DefaultRootExpirationConfiguration { get; } = entry =>
        {
            entry.Size = 0;
        };
    }
    /// <summary>
    /// A convenience-implementation of the ILinkHcoCache-interface backed by an IMemoryCache
    /// Entries are stored to the IMemoryCache with keys that are a tuple of the TUserIdentifier and the Uri
    /// To enable clearing of the user-cache and the shared cache change tokens are registered for each user
    /// This store can be cleared by calling the static ClearTokenStore method, which will also remove any entry
    /// in the IMemoryCache added by any instance of the LinkHcoMemoryUserCache
    /// </summary>
    /// <typeparam name="TUserIdentifier">An identifier type to tell users apart. Choose in a way, that no two different users in your system will have the same identifier, and such that no other service enters tuples of (TIdentifier, Uri) into the IMemoryCache</typeparam>
    /// <typeparam name="TValidator"></typeparam>
    public class LinkHcoMemoryUserCache<TUserIdentifier, TValidator>
        : ILinkHcoCache<TValidator>
    {
        private static readonly PostEvictionDelegate wrapperEvictionDelegate = (
            key,
            value,
            reason,
            state) =>
        {
            if (value is CancellationChangeTokenWrapper wrapper)
            {
                wrapper.Cancel();
                wrapper.Dispose();
            }
        };

        private readonly IMemoryCache memoryCache;
        private readonly TUserIdentifier currentUserIdentifier;
        private readonly TUserIdentifier sharedUserIdentifier;
        private readonly Func<TUserIdentifier, Uri, object> hcoEntryKeyBuilder;
        private readonly Func<TUserIdentifier, object> controlEntryKeyBuilder;
        private readonly Action<ICacheEntry, LinkHcoCacheEntry<TValidator>> configureEntryExpiration;
        private readonly Action<ICacheEntry> configureControlExpiration;
        private readonly Action<ICacheEntry> configureRootExpiration;
        private readonly object rootControlTokenKey;

        /// <summary>
        /// Create an instance of an ILinkHcoCache backed by an IMemoryCache
        /// </summary>
        /// <param name="memoryCache">The IMemoryCache to hold the actual values</param>
        /// <param name="currentUserIdentifier">Identifies the current user</param>
        /// <param name="sharedUserIdentifier">Placeholder identifier to cache items that can be used between users</param>
        /// <param name="hcoEntryKeyBuilder"></param>
        /// <param name="controlEntryKeyBuilder"></param>
        /// <param name="configureEntryExpiration">Use to set up entries with AbsoluteExpiration and SlidingExpiration</param>
        /// <param name="configureControlExpiration"></param>
        /// <param name="configureRootExpiration"></param>
        /// <param name="rootControlTokenKey"></param>
        public LinkHcoMemoryUserCache(
            IMemoryCache memoryCache,
            TUserIdentifier currentUserIdentifier,
            TUserIdentifier sharedUserIdentifier,
            Func<TUserIdentifier, Uri, object> hcoEntryKeyBuilder,
            Func<TUserIdentifier, object> controlEntryKeyBuilder,
            Action<ICacheEntry, LinkHcoCacheEntry<TValidator>> configureEntryExpiration,
            Action<ICacheEntry> configureControlExpiration,
            Action<ICacheEntry> configureRootExpiration,
            object rootControlTokenKey)
        {
            this.memoryCache = memoryCache;
            this.currentUserIdentifier = currentUserIdentifier;
            this.sharedUserIdentifier = sharedUserIdentifier;
            this.hcoEntryKeyBuilder = hcoEntryKeyBuilder;
            this.controlEntryKeyBuilder = controlEntryKeyBuilder;
            this.configureEntryExpiration = configureEntryExpiration;
            this.configureControlExpiration = configureControlExpiration;
            this.configureRootExpiration = configureRootExpiration;
            this.rootControlTokenKey = rootControlTokenKey;
        }

        public bool TryGetValue(
            Uri uri,
            out LinkHcoCacheEntry<TValidator> entry)
        {
            if (this.memoryCache.TryGetValue(this.hcoEntryKeyBuilder(this.currentUserIdentifier, uri), out entry))
            {
                return true;
            }

            return this.memoryCache.TryGetValue(this.hcoEntryKeyBuilder(this.sharedUserIdentifier, uri), out entry);
        }

        public void Set(
            Uri uri,
            LinkHcoCacheEntry<TValidator> entry)
        {
            TUserIdentifier userIdentifier;
            switch (entry.CacheScope)
            {
                case CacheScope.ForIndividualUserContext:
                case CacheScope.Undefined:
                default:
                    userIdentifier = this.currentUserIdentifier;
                    break;
                case CacheScope.AcrossUserContexts:
                    userIdentifier = this.sharedUserIdentifier;
                    break;
            }

            var userClearTokenKey = this.controlEntryKeyBuilder(userIdentifier);
            var userClearToken = GetUserClearToken(userClearTokenKey);

            using (var e = this.memoryCache.CreateEntry(this.hcoEntryKeyBuilder(userIdentifier, uri)))
            {
                e.Value = entry;
                e.ExpirationTokens.Add(userClearToken.Token);
                this.configureEntryExpiration?.Invoke(e, entry);
            }
        }

        private CancellationChangeTokenWrapper GetUserClearToken(object userClearTokenKey)
        {
            CancellationChangeTokenWrapper userClearToken;
            // use the while loop here instead of GetOrAdd because someone else might overwrite the wrapper at the same time, and the local reference might be invalid because of the post eviction dispose.
            while (!this.memoryCache.TryGetValue(userClearTokenKey, out userClearToken))
            {
                CancellationChangeTokenWrapper rootClearToken;
                while (!this.memoryCache.TryGetValue(this.rootControlTokenKey, out rootClearToken))
                {
                    using (var e = this.memoryCache.CreateEntry(this.rootControlTokenKey))
                    {
                        e.Value = new CancellationChangeTokenWrapper();
                        e.PostEvictionCallbacks.Add(
                            new PostEvictionCallbackRegistration()
                            {
                                EvictionCallback = wrapperEvictionDelegate,
                            });
                        this.configureRootExpiration?.Invoke(e);
                    }
                }
                using (var e = this.memoryCache.CreateEntry(userClearTokenKey))
                {
                    var wrapper = new CancellationChangeTokenWrapper();
                    e.Value = wrapper;
                    e.PostEvictionCallbacks.Add(
                        new PostEvictionCallbackRegistration()
                        {
                            EvictionCallback = wrapperEvictionDelegate,
                        });
                    e.ExpirationTokens.Add(rootClearToken.Token);
                    this.configureControlExpiration?.Invoke(e);
                }
            }

            return userClearToken;
        }

        public void Remove(Uri uri)
        {
            this.memoryCache.Remove(this.hcoEntryKeyBuilder(this.sharedUserIdentifier, uri));
            this.memoryCache.Remove(this.hcoEntryKeyBuilder(this.currentUserIdentifier, uri));
        }

        public void Clear()
        {
            this.memoryCache.Remove(this.controlEntryKeyBuilder(this.currentUserIdentifier));
            this.memoryCache.Remove(this.controlEntryKeyBuilder(this.sharedUserIdentifier));
        }
    }
}
