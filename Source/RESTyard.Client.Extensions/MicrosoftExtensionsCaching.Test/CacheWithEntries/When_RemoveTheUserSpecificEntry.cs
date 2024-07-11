using FluentAssertions;
using Xunit;

namespace Extensions.Test.Caching.CacheWithEntries;

public class When_RemoveTheUserSpecificEntry : Given_ACacheWithEntries
{
    public When_RemoveTheUserSpecificEntry()
    {
        this.UserCache.Remove(this.TestUri);
    }

    [Fact]
    public void Then_TheSharedEntryIsStillThere()
    {
        var success = this.UserCache.TryGetValue(this.SharedEntryUri, out var entry);

        success.Should().BeTrue();
        entry!.LinkResponseContent.Should().BeEquivalentTo(this.SharedEntry);
    }

    [Fact]
    public void Then_TheUserSpecificEntryIsRemoved()
    {
        this.UserCache.TryGetValue(this.TestUri, out _).Should().BeFalse();
    }
}