using System;
using Bluehands.Hypermedia.Client.Hypermedia;
using Bluehands.Hypermedia.Client.ParameterSerializer;
using Bluehands.Hypermedia.Client.Reader;
using Bluehands.Hypermedia.Client.Resolver;

namespace Bluehands.Hypermedia.Client
{
    public class HypermediaClientBuilder
    {
        private Func<IHypermediaObjectRegister> createHypermediaObjectRegister;
        private Func<IParameterSerializer> createParameterSerializer;
        private Func<IStringParser> createStringParser;
        private Func<IProblemStringReader> createProblemStringReader;
        private Func<IParameterSerializer, IProblemStringReader, IHypermediaResolver> createHypermediaResolver;
        private Func<IHypermediaObjectRegister, IStringParser, IHypermediaReader> createHypermediaReader;

        public HypermediaClientBuilder()
        {

        }

        private static T Get<T>(Func<T> getFunc, params string[] possibleMethodNames)
        {
            if (getFunc == null)
            {
                throw new InvalidOperationException(
                    $"Please call any of {string.Join(",", possibleMethodNames)}, or a suitable extension method from one of the Hypermedia.Client.Extensions packages, before creating the HypermediaClient");
            }

            return getFunc();
        }

        public HypermediaClient<TEntryPoint> CreateHypermediaClient<TEntryPoint>(Uri uriApiEntryPoint) where TEntryPoint : HypermediaClientObject
        {
            var objectRegister = Get(this.createHypermediaObjectRegister, nameof(ConfigureObjectRegister));
            var serializer = Get(this.createParameterSerializer, nameof(WithCustomParameterSerializer));
            var stringParser = Get(this.createStringParser, nameof(WithCustomStringParser));
            var problemReader = Get(this.createProblemStringReader, nameof(WithCustomProblemStringReader));
            var resolver = Get(() => this.createHypermediaResolver(serializer, problemReader), nameof(WithCustomHypermediaResolver));
            var reader = Get(() => this.createHypermediaReader(objectRegister, stringParser), nameof(WithSirenHypermediaReader), nameof(WithCustomHypermediaReader));
            resolver.InitializeHypermediaReader(reader);
            reader.InitializeHypermediaResolver(resolver);
            var client = new HypermediaClient<TEntryPoint>(uriApiEntryPoint, resolver, reader);
            return client;
        }

        public HypermediaClientBuilder ConfigureObjectRegister(Action<HypermediaObjectRegister> configure)
        {
            this.createHypermediaObjectRegister = () =>
            {
                var register = new HypermediaObjectRegister();
                configure(register);
                return register;
            };
            return this;
        }

        public HypermediaClientBuilder WithCustomParameterSerializer(Func<IParameterSerializer> createParameterSerializer)
        {
            this.createParameterSerializer = createParameterSerializer;
            return this;
        }

        public HypermediaClientBuilder WithCustomHypermediaResolver(Func<IParameterSerializer, IProblemStringReader, IHypermediaResolver> createResolver)
        {
            this.createHypermediaResolver = createResolver;
            return this;
        }

        public HypermediaClientBuilder WithCustomStringParser(Func<IStringParser> createStringParser)
        {
            this.createStringParser = createStringParser;
            return this;
        }

        public HypermediaClientBuilder WithCustomProblemStringReader(Func<IProblemStringReader> createProblemStringReader)
        {
            this.createProblemStringReader = createProblemStringReader;
            return this;
        }

        public HypermediaClientBuilder WithCustomHypermediaReader(Func<IHypermediaObjectRegister, IStringParser, IHypermediaReader> createHypermediaReader)
        {
            this.createHypermediaReader = createHypermediaReader;
            return this;
        }

        public HypermediaClientBuilder WithSirenHypermediaReader()
        {
            this.createHypermediaReader = (register, parser) => new SirenHypermediaReader(register, parser);
            return this;
        }
    }
}