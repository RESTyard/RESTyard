using System;
using RESTyard.Client.Extensions;

namespace RESTyard.Client.Resolver.Caching
{
    public abstract record CacheEntryVerificationResult<TNetworkResponseMessage>
    {
        public void Match(
            Action<CacheEntryMayBeUsed> mayBeUsed,
            Action<CacheEntryMayNotBeUsed> mayNotBeUsed,
            Action<UseThisResponseInstead> useResponse)
            => this.TypeMatch(mayBeUsed, mayNotBeUsed, useResponse);

        public TMatchResult Match<TMatchResult>(
            Func<CacheEntryMayBeUsed, TMatchResult> mayBeUsed,
            Func<CacheEntryMayNotBeUsed, TMatchResult> mayNotBeUsed,
            Func<UseThisResponseInstead, TMatchResult> useResponse)
            => this.TypeMatch(mayBeUsed, mayNotBeUsed, useResponse);

        public sealed record CacheEntryMayBeUsed : CacheEntryVerificationResult<TNetworkResponseMessage>;
        public sealed record CacheEntryMayNotBeUsed : CacheEntryVerificationResult<TNetworkResponseMessage>;
        public sealed record UseThisResponseInstead
            (TNetworkResponseMessage Response) : CacheEntryVerificationResult<TNetworkResponseMessage>;
    }
}