using FunicularSwitch.Generators;

namespace RESTyard.Client.Resolver.Caching;

[UnionType(CaseOrder = CaseOrder.AsDeclared)]
public abstract partial record CacheEntryVerificationResult<TNetworkResponseMessage>
{
    public sealed record CacheEntryMayBeUsed_ : CacheEntryVerificationResult<TNetworkResponseMessage>;
    public sealed record CacheEntryMayNotBeUsed_ : CacheEntryVerificationResult<TNetworkResponseMessage>;
    public sealed record UseThisResponseInstead_(TNetworkResponseMessage Response) : CacheEntryVerificationResult<TNetworkResponseMessage>;
}