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

        /// <summary>
        /// Build the HypermediaClient using all the configured interface implementations
        /// </summary>
        /// <typeparam name="TEntryPoint"></typeparam>
        /// <param name="uriApiEntryPoint"></param>
        /// <exception cref="System.InvalidOperationException">Thrown if not all necessary configurations were made</exception>
        /// <returns></returns>
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

        /// <summary>
        /// Use to build the IHypermediaObjectRegister by registering all HCO types that are to be received
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        public HypermediaClientBuilder ConfigureObjectRegister(Action<IHypermediaObjectRegister> configure)
        {
            this.createHypermediaObjectRegister = () =>
            {
                var register = new HypermediaObjectRegister();
                configure(register);
                return register;
            };
            return this;
        }

        /// <summary>
        /// Internal extension point for injecting an IParameterSerializer implementation
        /// </summary>
        /// <param name="createParameterSerializer"></param>
        /// <returns></returns>
        public HypermediaClientBuilder WithCustomParameterSerializer(Func<IParameterSerializer> createParameterSerializer)
        {
            this.createParameterSerializer = createParameterSerializer;
            return this;
        }

        /// <summary>
        /// Internal extension point to inject an IHypermediaResolver implementation
        /// </summary>
        /// <param name="createResolver"></param>
        /// <returns></returns>
        public HypermediaClientBuilder WithCustomHypermediaResolver(Func<IParameterSerializer, IProblemStringReader, IHypermediaResolver> createResolver)
        {
            this.createHypermediaResolver = createResolver;
            return this;
        }

        /// <summary>
        /// Internal extension point to inject an IStringParser implementation
        /// </summary>
        /// <param name="createStringParser"></param>
        /// <returns></returns>
        public HypermediaClientBuilder WithCustomStringParser(Func<IStringParser> createStringParser)
        {
            this.createStringParser = createStringParser;
            return this;
        }

        /// <summary>
        /// Internal extension point to inject an IProblemStringReader implementation.
        /// </summary>
        /// <param name="createProblemStringReader"></param>
        /// <returns></returns>
        public HypermediaClientBuilder WithCustomProblemStringReader(Func<IProblemStringReader> createProblemStringReader)
        {
            this.createProblemStringReader = createProblemStringReader;
            return this;
        }

        /// <summary>
        /// Internal extension point to inject an IHypermediaReader implementation
        /// </summary>
        /// <param name="createHypermediaReader"></param>
        /// <returns></returns>
        public HypermediaClientBuilder WithCustomHypermediaReader(Func<IHypermediaObjectRegister, IStringParser, IHypermediaReader> createHypermediaReader)
        {
            this.createHypermediaReader = createHypermediaReader;
            return this;
        }

        /// <summary>
        /// Use the SIREN format (https://github.com/kevinswiber/siren) to read incoming data
        /// </summary>
        /// <returns></returns>
        public HypermediaClientBuilder WithSirenHypermediaReader()
        {
            this.createHypermediaReader = (register, parser) => new SirenHypermediaReader(register, parser);
            return this;
        }
    }
}