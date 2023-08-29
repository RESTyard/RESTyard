using System;
using RESTyard.Client.ParameterSerializer;
using RESTyard.Client.Reader;
using RESTyard.Client.Resolver.Caching;

namespace RESTyard.Client.Builder
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

        new IHypermediaResolverDependencies<TLinkHcoCacheEntry> BuildDependencies();
    }
}