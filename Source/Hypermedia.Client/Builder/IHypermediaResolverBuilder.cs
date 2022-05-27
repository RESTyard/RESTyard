using System;
using Bluehands.Hypermedia.Client.ParameterSerializer;
using Bluehands.Hypermedia.Client.Reader;
using Bluehands.Hypermedia.Client.Resolver.Caching;

namespace Bluehands.Hypermedia.Client.Builder
{
    public interface IHypermediaResolverBuilder
    {
        IHypermediaResolverBuilder ConfigureObjectRegister(Action<IHypermediaObjectRegister> configure);

        IHypermediaResolverBuilder WithCustomParameterSerializer(Func<IParameterSerializer> createParameterSerializer);

        IHypermediaResolverBuilder WithCustomStringParser(Func<IStringParser> createStringParser);

        IHypermediaResolverBuilder WithCustomProblemStringReader(Func<IProblemStringReader> createProblemStringReader);

        IHypermediaResolverBuilder WithCustomHypermediaReader(
            Func<IHypermediaObjectRegister, IStringParser, IHypermediaReader> createHypermediaReader);

        IHypermediaResolverDependencies BuildDependencies();
    }

    public interface IHypermediaResolverBuilder<TLinkHcoCacheEntry>
        : IHypermediaResolverBuilder
        where TLinkHcoCacheEntry : LinkHcoCacheEntry
    {
        IHypermediaResolverBuilder<TLinkHcoCacheEntry> WithCustomLinkHcoCache(
            Func<ILinkHcoCache<TLinkHcoCacheEntry>> createCache);

        IHypermediaResolverDependencies<TLinkHcoCacheEntry> BuildDependencies();
    }
}