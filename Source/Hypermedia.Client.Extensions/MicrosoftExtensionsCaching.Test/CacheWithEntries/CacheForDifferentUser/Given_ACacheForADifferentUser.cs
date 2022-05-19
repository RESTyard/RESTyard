﻿using Bluehands.Hypermedia.Client.Resolver.Caching;
using MicrosoftExtensionsCaching;

namespace Extensions.Test.Caching.CacheWithEntries.CacheForDifferentUser;

public class Given_ACacheForADifferentUser : Given_ACacheWithEntries
{
    protected ILinkHcoCache<string> OtherUserCache { get; }

    public Given_ACacheForADifferentUser()
    {
        this.OtherUserCache = new LinkHcoMemoryUserCache<string, string>(
            this.MemoryCache,
            "OtherUser",
            SharedUserIdentifier,
            this.HcoEntryKeyBuilder,
            this.ControlEntryKeyBuilder,
            this.ConfigureEntryExpiration,
            this.ConfigureControlExpiration,
            this.ConfigureRootExpiration,
            RootControlTokenKey);
    }
}