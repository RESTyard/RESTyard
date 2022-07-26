using System;
using RESTyard.Client.ParameterSerializer;
using RESTyard.Client.Reader;
using RESTyard.Client.Resolver.Caching;

namespace RESTyard.Client.Builder
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