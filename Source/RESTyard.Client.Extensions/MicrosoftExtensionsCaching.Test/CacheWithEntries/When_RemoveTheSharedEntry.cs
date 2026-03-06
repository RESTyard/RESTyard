using AwesomeAssertions;
using Xunit;

namespace Extensions.Test.Caching.CacheWithEntries;

public class When_RemoveTheSharedEntry : Given_ACacheWithEntries
{
    public When_RemoveTheSharedEntry()
    {
        this.UserCache.Remove(this.SharedEntryUri);
    }

    [Fact]
    public void Then_TheSharedEntryIsRemoved()
    {
        this.UserCache.TryGetValue(this.SharedEntryUri, out _).Should().BeFalse();
    }

    [Fact]
    public void Then_TheUserSpecificEntryIsStillThere()
    {
        var success = this.UserCache.TryGetValue(this.TestUri, out var entry);
        success.Should().BeTrue();
        entry!.LinkResponseContent.Should().BeEquivalentTo(this.TestHco);
    }
}