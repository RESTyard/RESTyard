using System;
using Bluehands.Hypermedia.Client.Hypermedia;
using Bluehands.Hypermedia.Client.ParameterSerializer;
using Bluehands.Hypermedia.Client.Reader;
using Bluehands.Hypermedia.Client.Resolver;

namespace Bluehands.Hypermedia.Client
{
    public class HypermediaResolverBuilder
    {
        private Func<IHypermediaObjectRegister> createHypermediaObjectRegister;
        private Func<IParameterSerializer> createParameterSerializer;
        private Func<IStringParser> createStringParser;
        private Func<IProblemStringReader> createProblemStringReader;
        private Func<IHypermediaObjectRegister, IStringParser, IHypermediaReader> createHypermediaReader;

        public HypermediaResolverBuilder()
        {

        }

        private static T Get<T>(Func<T> getFunc, params string[] possibleMethodNames)
        {
            if (getFunc == null)
            {
                throw new InvalidOperationException(
                    $"Please call any of {string.Join(",", possibleMethodNames)}, or a suitable extension method from one of the Hypermedia.Client.Extensions packages, before creating the {nameof(IHypermediaResolver)}");
            }

            return getFunc();
        }

        /// <summary>
        /// Internal extension point exposing the configured objects to build the IHypermediaResolver with
        /// </summary>
        /// <returns></returns>
        public IResolverDependencies BuildDependencies()
        {
            var objectRegister = Get(this.createHypermediaObjectRegister, nameof(ConfigureObjectRegister));
            var serializer = Get(this.createParameterSerializer, nameof(WithCustomParameterSerializer));
            var stringParser = Get(this.createStringParser, nameof(WithCustomStringParser));
            var problemReader = Get(this.createProblemStringReader, nameof(WithCustomProblemStringReader));
            var reader = Get(() => this.createHypermediaReader(objectRegister, stringParser), nameof(WithSirenHypermediaReader), nameof(WithCustomHypermediaReader));
            return new ResolverDependencies(objectRegister, serializer, stringParser, problemReader, reader);
        }

        public interface IResolverDependencies
        {
            IHypermediaObjectRegister ObjectRegister { get; }

            IParameterSerializer ParameterSerializer { get; }

            IStringParser StringParser { get; }

            IProblemStringReader ProblemReader { get; }

            IHypermediaReader HypermediaReader { get; }
        }

        private class ResolverDependencies : IResolverDependencies
        {
            public ResolverDependencies(IHypermediaObjectRegister objectRegister,
                IParameterSerializer parameterSerializer,
                IStringParser stringParser,
                IProblemStringReader problemReader,
                IHypermediaReader hypermediaReader)
            {
                ObjectRegister = objectRegister;
                ParameterSerializer = parameterSerializer;
                StringParser = stringParser;
                ProblemReader = problemReader;
                HypermediaReader = hypermediaReader;
            }

            public IHypermediaObjectRegister ObjectRegister { get; }
            public IParameterSerializer ParameterSerializer { get; }
            public IStringParser StringParser { get; }
            public IProblemStringReader ProblemReader { get; }
            public IHypermediaReader HypermediaReader { get; }
        }

        /// <summary>
        /// Use to build the IHypermediaObjectRegister by registering all HCO types that are to be received
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        public HypermediaResolverBuilder ConfigureObjectRegister(Action<IHypermediaObjectRegister> configure)
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
        public HypermediaResolverBuilder WithCustomParameterSerializer(Func<IParameterSerializer> createParameterSerializer)
        {
            this.createParameterSerializer = createParameterSerializer;
            return this;
        }

        /// <summary>
        /// Internal extension point to inject an IStringParser implementation
        /// </summary>
        /// <param name="createStringParser"></param>
        /// <returns></returns>
        public HypermediaResolverBuilder WithCustomStringParser(Func<IStringParser> createStringParser)
        {
            this.createStringParser = createStringParser;
            return this;
        }

        /// <summary>
        /// Internal extension point to inject an IProblemStringReader implementation.
        /// </summary>
        /// <param name="createProblemStringReader"></param>
        /// <returns></returns>
        public HypermediaResolverBuilder WithCustomProblemStringReader(Func<IProblemStringReader> createProblemStringReader)
        {
            this.createProblemStringReader = createProblemStringReader;
            return this;
        }

        /// <summary>
        /// Internal extension point to inject an IHypermediaReader implementation
        /// </summary>
        /// <param name="createHypermediaReader"></param>
        /// <returns></returns>
        public HypermediaResolverBuilder WithCustomHypermediaReader(Func<IHypermediaObjectRegister, IStringParser, IHypermediaReader> createHypermediaReader)
        {
            this.createHypermediaReader = createHypermediaReader;
            return this;
        }

        /// <summary>
        /// Use the SIREN format (https://github.com/kevinswiber/siren) to read incoming data
        /// </summary>
        /// <returns></returns>
        public HypermediaResolverBuilder WithSirenHypermediaReader()
        {
            this.createHypermediaReader = (register, parser) => new SirenHypermediaReader(register, parser);
            return this;
        }
    }
}