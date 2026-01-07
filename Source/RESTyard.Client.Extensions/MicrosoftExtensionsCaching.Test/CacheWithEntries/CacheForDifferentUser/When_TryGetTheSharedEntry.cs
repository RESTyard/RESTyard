using AwesomeAssertions;
using RESTyard.Client.Resolver.Caching;
using Xunit;

namespace Extensions.Test.Caching.CacheWithEntries.CacheForDifferentUser;

public class When_TryGetTheSharedEntry : Given_ACacheForADifferentUser
{
    private bool success;
    private LinkHcoCacheEntry entry;
    public When_TryGetTheSharedEntry()
    {
        this.success = this.OtherUserCache.TryGetValue(this.SharedEntryUri, out entry);
    }

    [Fact]
    public void Then_TheObjectIsRetrieved()
    {
        this.success.Should().BeTrue();
        this.entry.LinkResponseContent.Should().BeEquivalentTo(this.SharedEntry);
    }
}