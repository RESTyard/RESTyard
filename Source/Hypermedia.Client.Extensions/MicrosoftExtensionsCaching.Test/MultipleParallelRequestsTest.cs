using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bluehands.Hypermedia.Client.Hypermedia;
using Bluehands.Hypermedia.Client.Resolver.Caching;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using MicrosoftExtensionsCaching;
using Xunit;

namespace Extensions.Test.Caching;

public class MultipleParallelRequestsTest : LinkHcoMemoryUserCacheTestBase
{
    private const int NumberOfThreads = 20;

    private readonly IList<string> userIdentifiers;

    private readonly IList<ILinkHcoCache<LinkHcoCacheEntry>> userCaches;

    private readonly IList<Uri> uriCollection;

    private readonly Random random;
    private readonly object randomLock;

    public MultipleParallelRequestsTest()
        : base(new MemoryCacheOptions()
        {
            SizeLimit = 2000,
        })
    {
        this.userIdentifiers = Enumerable.Range(1, NumberOfThreads).Select(x => $"UserIdentifier{x}").ToList();
        var factory = new LinkHcoMemoryUserCacheFactory<string, LinkHcoCacheEntry>(
            this.MemoryCache,
            SharedUserIdentifier,
            HcoEntryKeyBuilder,
            ControlEntryKeyBuilder,
            (iEntry, entry) => iEntry.Size = 1,
            entry => entry.Size = 0,
            entry => entry.Size = 0,
            RootControlTokenKey);
        this.userCaches = userIdentifiers
            .Select(factory.CreateUserCache)
            .ToList();

        this.uriCollection = Enumerable
            .Range(1, 100 * 1000)
            .Select(x => x % 100)
            .Select(x => new Uri($"payload://uri{x}")).ToList();
        this.random = new Random(20938745);
        this.randomLock = new object();
    }

    private int NextInt()
    {
        lock (randomLock)
        {
            return this.random.Next();
        }
    }

    [Fact]
    public async Task Given_MultipleCaches_When_RequestsAreMadeInParallel()
    {
        var threads = new Thread[NumberOfThreads];
        var results = new int[NumberOfThreads];
        for (int i = 0; i < NumberOfThreads; i += 1)
        {
            var randomPayload = this.uriCollection
                .OrderBy(x => this.random.Next()).ToList();
            int threadNumber = i;
            threads[threadNumber] = new Thread(
                () =>
                {
                    int sharedCounter = 0;
                    int successCounter = 0;
                    foreach (var uri in randomPayload)
                    {
                        sharedCounter = (sharedCounter + 1) % 10;
                        if (this.userCaches[threadNumber].TryGetValue(uri, out var entry))
                        {
                            successCounter += 1;
                        }
                        else
                        {
                            this.userCaches[threadNumber].Set(
                                uri,
                                new LinkHcoCacheEntry(
                                    $"{threadNumber}-{NextInt()}",
                                    sharedCounter == 0
                                        ? CacheScope.AcrossUserContexts
                                        : CacheScope.ForIndividualUserContext,
                                    null));
                        }
                    }

                    results[threadNumber] = successCounter;
                });
        }

        foreach (var t in threads)
        {
            t.Start();
        }

        foreach (var t in threads)
        {
            t.Join();
        }

        var memCache = this.MemoryCache;
        memCache.Count.Should().BeLessThanOrEqualTo((int)this.MemoryCacheOptions.Value.SizeLimit + this.userCaches.Count + 2);
        memCache.Count.Should().BeGreaterThan(0);
        foreach (var uc in this.userCaches)
        {
            uc.Clear();
        }

        await Task.Delay(TimeSpan.FromMilliseconds(10));
        memCache.Count.Should().Be(1);
        LinkHcoMemoryUserCache.ClearAllLinkHcoCacheEntries(this.MemoryCache, RootControlTokenKey);
        await Task.Delay(TimeSpan.FromMilliseconds(10));
        memCache.Count.Should().Be(0);
    }
}