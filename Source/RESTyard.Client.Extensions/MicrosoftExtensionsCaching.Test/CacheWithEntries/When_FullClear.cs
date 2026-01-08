using System;
using System.Threading.Tasks;
using AwesomeAssertions;
using MicrosoftExtensionsCaching;
using Xunit;

namespace Extensions.Test.Caching.CacheWithEntries;

public class When_FullClear : Given_ACacheWithEntries
{
    public When_FullClear()
    {
        LinkHcoMemoryUserCache.ClearAllLinkHcoCacheEntries(this.MemoryCache, RootControlTokenKey);
    }

    [Fact]
    public async Task Then_TheSharedEntryIsRemoved()
    {
        // The Post eviction callback is called asynchronously, so we have to wait here for a moment
        await Task.Delay(TimeSpan.FromMilliseconds(10));
        this.UserCache.TryGetValue(this.SharedEntryUri, out _).Should().BeFalse();
    }

    [Fact]
    public async Task Then_TheUserSpecificEntryIsRemoved()
    {
        // The Post eviction callback is called asynchronously, so we have to wait here for a moment
        await Task.Delay(TimeSpan.FromMilliseconds(10));
        this.UserCache.TryGetValue(this.TestUri, out _).Should().BeFalse();
    }
}