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
        private Func<IParameterSerializer, IHypermediaResolver> createHypermediaResolver;
        private Func<IHypermediaObjectRegister, IStringParser, IHypermediaReader> createHypermediaReader;

        public HypermediaClientBuilder()
        {

        }

        public HypermediaClient<TEntryPoint> CreateHypermediaClient<TEntryPoint>(Uri uriApiEntryPoint) where TEntryPoint : HypermediaClientObject
        {
            var objectRegister = this.createHypermediaObjectRegister();
            var serializer = this.createParameterSerializer();
            var stringParser = this.createStringParser();
            var resolver = this.createHypermediaResolver(serializer);
            var reader = this.createHypermediaReader(objectRegister, stringParser);
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

        public HypermediaClientBuilder WithCustomParameterSerializer(IParameterSerializer parameterSerializer)
        {
            this.createParameterSerializer = () => parameterSerializer;
            return this;
        }

        public HypermediaClientBuilder WithDefaultParameterSerializer()
        {
            this.createParameterSerializer = () => new JsonObjectParameterSerializer();
            return this;
        }

        public HypermediaClientBuilder WithSingleJsonParameterSerializer()
        {
            this.createParameterSerializer = () => new SingleJsonObjectParameterSerializer();
            return this;
        }

        public HypermediaClientBuilder WithCustomResolver(IHypermediaResolver resolver)
        {
            this.createHypermediaResolver = _ => resolver;
            return this;
        }

        public HypermediaClientBuilder WithHttpResolver(Action<HttpHypermediaResolver> configure)
        {
            this.createHypermediaResolver = serializer =>
            {
                var resolver = new HttpHypermediaResolver(serializer, NoLinkCache<string>.Instance);
                configure(resolver);
                return resolver;
            };
            return this;
        }

        public HypermediaClientBuilder WithHttpResolver(ILinkHcoCache<string> linkHcoCache, Action<HttpHypermediaResolver> configure)
        {
            this.createHypermediaResolver = serializer =>
            {
                var resolver = new HttpHypermediaResolver(serializer, linkHcoCache);
                configure(resolver);
                return resolver;
            };
            return this;
        }

        public HypermediaClientBuilder WithCustomStringParser(IStringParser parser)
        {
            this.createStringParser = () => parser;
            return this;
        }

        public HypermediaClientBuilder WithNewtonsoftJsonReader()
        {
            this.createStringParser = () => new NewtonsoftJsonStringParser();
            return this;
        }

        public HypermediaClientBuilder WithCustomHypermediaReader(IHypermediaReader reader)
        {
            this.createHypermediaReader = (_, __) => reader;
            return this;
        }

        public HypermediaClientBuilder WithSirenHypermediaReader()
        {
            this.createHypermediaReader = (register, parser) => new SirenHypermediaReader(register, parser);
            return this;
        }
    }
}