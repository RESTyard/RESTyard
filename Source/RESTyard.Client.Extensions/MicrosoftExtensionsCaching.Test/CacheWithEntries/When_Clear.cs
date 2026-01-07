using System;
using System.Threading.Tasks;
using AwesomeAssertions;
using Xunit;

namespace Extensions.Test.Caching.CacheWithEntries;

public class When_Clear : Given_ACacheWithEntries
{
    public When_Clear()
    {
        this.UserCache.Clear();
    }

    [Fact]
    public async Task Then_TheUserSpecificEntryIsRemoved()
    {
        // The Post eviction callback is called asynchronously, so we have to wait here for a moment
        await Task.Delay(TimeSpan.FromMilliseconds(10));
        this.UserCache.TryGetValue(this.TestUri, out _).Should().BeFalse();
    }
}