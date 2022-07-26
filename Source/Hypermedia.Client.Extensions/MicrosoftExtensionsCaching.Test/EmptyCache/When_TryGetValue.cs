﻿using System;
using System.Threading.Tasks;
using FluentAssertions;
using RESTyard.Client.Resolver.Caching;
using Xunit;

namespace Extensions.Test.Caching.EmptyCache;

public class When_TryGetValue : Given_AnEmptyCache
{
    private readonly bool success;
    private LinkHcoCacheEntry entry;

    public When_TryGetValue()
    {
        this.success = this.UserCache.TryGetValue(this.TestUri, out this.entry);
    }

    [Fact]
    public void Then_SuccessIsFalse()
    {
        this.success.Should().BeFalse();
    }
}