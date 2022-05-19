using System;
using Bluehands.Hypermedia.Client.Resolver.Caching;

namespace Extensions.Test.Caching.CacheWithEntries;

public class Given_ACacheWithEntries : LinkHcoMemoryUserCacheTestBase
{
    protected Uri SharedEntryUri => new Uri("shared://some_path");

    protected string SharedEntry => "SharedEntry";

    public Given_ACacheWithEntries()
    {
        this.UserCache.Set(
            this.TestUri,
            this.CreateEntry(
                this.TestHco,
                CacheScope.ForIndividualUserContext));
        this.UserCache.Set(
            this.SharedEntryUri,
            this.CreateEntry(
                this.SharedEntry,
                CacheScope.AcrossUserContexts));
    }
}