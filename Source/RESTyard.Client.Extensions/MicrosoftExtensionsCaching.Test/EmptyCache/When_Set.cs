using FluentAssertions;
using RESTyard.Client.Resolver.Caching;
using Xunit;

namespace Extensions.Test.Caching.EmptyCache;

public class When_SetForIndividualUser : Given_AnEmptyCache
{
    public When_SetForIndividualUser()
    {
        this.UserCache.Set(
            this.TestUri,
            this.CreateEntry(
                this.TestHco,
                CacheScope.ForIndividualUserContext));
    }

    [Fact]
    public void Then_TheObjectIsRetrieved()
    {
        var success = this.UserCache.TryGetValue(this.TestUri, out var entry);
        success.Should().BeTrue();
        entry.LinkResponseContent.Should().BeEquivalentTo(this.TestHco);
    }
}

public class When_SetForSharedUser : Given_AnEmptyCache
{
    public When_SetForSharedUser()
    {
        this.UserCache.Set(
            this.TestUri,
            this.CreateEntry(
                this.TestHco,
                CacheScope.AcrossUserContexts));
    }

    [Fact]
    public void Then_TheObjectIsRetrieved()
    {
        var success = this.UserCache.TryGetValue(this.TestUri, out var entry);
        success.Should().BeTrue();
        entry.LinkResponseContent.Should().BeEquivalentTo(this.TestHco);
    }
}