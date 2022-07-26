using MicrosoftExtensionsCaching;
using RESTyard.Client.Resolver.Caching;

namespace Extensions.Test.Caching.CacheWithEntries.CacheForDifferentUser;

public class Given_ACacheForADifferentUser : Given_ACacheWithEntries
{
    protected ILinkHcoCache<LinkHcoCacheEntry> OtherUserCache { get; }

    public Given_ACacheForADifferentUser()
    {
        this.OtherUserCache = new LinkHcoMemoryUserCache<string, LinkHcoCacheEntry>(
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