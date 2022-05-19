using Bluehands.Hypermedia.Client.Resolver.Caching;
using FluentAssertions;
using Xunit;

namespace Extensions.Test.Caching.CacheWithEntries.CacheForDifferentUser;

public class When_TryGetTheUserSpecificEntry : Given_ACacheForADifferentUser
{
    private bool success;
    private CacheEntry<string> entry;

    public When_TryGetTheUserSpecificEntry()
    {
        this.success = this.OtherUserCache.TryGetValue(this.TestUri, out entry);
    }

    [Fact]
    public void Then_TheObjectIsNotRetrieved()
    {
        this.success.Should().BeFalse();
    }
}