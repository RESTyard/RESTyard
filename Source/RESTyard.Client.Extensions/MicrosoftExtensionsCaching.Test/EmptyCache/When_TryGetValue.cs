using AwesomeAssertions;
using Xunit;

namespace Extensions.Test.Caching.EmptyCache;

public class When_TryGetValue : Given_AnEmptyCache
{
    private readonly bool success;

    public When_TryGetValue()
    {
        this.success = this.UserCache.TryGetValue(this.TestUri, out var _);
    }

    [Fact]
    public void Then_SuccessIsFalse()
    {
        this.success.Should().BeFalse();
    }
}