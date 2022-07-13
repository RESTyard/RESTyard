using System;
using Bluehands.Hypermedia.Client.Extensions;

namespace Bluehands.Hypermedia.Client.Resolver.Caching
{
    public abstract class CacheEntryVerificationResult<TNetworkResponseMessage>
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

        public sealed class CacheEntryMayBeUsed : CacheEntryVerificationResult<TNetworkResponseMessage>
        {
        }

        public sealed class CacheEntryMayNotBeUsed : CacheEntryVerificationResult<TNetworkResponseMessage>
        {
        }

        public sealed class UseThisResponseInstead : CacheEntryVerificationResult<TNetworkResponseMessage>
        {
            public UseThisResponseInstead(TNetworkResponseMessage response)
            {
                Response = response;
            }

            public TNetworkResponseMessage Response { get; }
        }
    }
}