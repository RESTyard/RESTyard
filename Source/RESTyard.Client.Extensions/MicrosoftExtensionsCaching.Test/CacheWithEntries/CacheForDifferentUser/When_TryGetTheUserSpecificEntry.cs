using AwesomeAssertions;
using RESTyard.Client.Resolver.Caching;
using Xunit;

namespace Extensions.Test.Caching.CacheWithEntries.CacheForDifferentUser;

public class When_TryGetTheUserSpecificEntry : Given_ACacheForADifferentUser
{
    private bool success;

    public When_TryGetTheUserSpecificEntry()
    {
        this.success = this.OtherUserCache.TryGetValue(this.TestUri, out var _);
    }

    [Fact]
    public void Then_TheObjectIsNotRetrieved()
    {
        this.success.Should().BeFalse();
    }
}