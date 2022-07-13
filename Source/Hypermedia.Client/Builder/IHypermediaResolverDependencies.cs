using Bluehands.Hypermedia.Client.ParameterSerializer;
using Bluehands.Hypermedia.Client.Reader;
using Bluehands.Hypermedia.Client.Resolver.Caching;

namespace Bluehands.Hypermedia.Client.Builder
{
    public interface IHypermediaResolverDependencies
    {
        IHypermediaObjectRegister ObjectRegister { get; }

        IParameterSerializer ParameterSerializer { get; }

        IStringParser StringParser { get; }

        IProblemStringReader ProblemReader { get; }

        IHypermediaReader HypermediaReader { get; }
    }

    public interface IHypermediaResolverDependencies<TLinkHcoCacheEntry>
        : IHypermediaResolverDependencies
        where TLinkHcoCacheEntry : LinkHcoCacheEntry
    {
        ILinkHcoCache<TLinkHcoCacheEntry> LinkHcoCache { get; }
    }
}