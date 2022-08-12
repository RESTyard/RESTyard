using System;
using RESTyard.Client.ParameterSerializer;
using RESTyard.Client.Reader;
using RESTyard.Client.Resolver;

namespace RESTyard.Client.Builder
{
    public static class HypermediaResolverBuilder
    {
        public static IHypermediaResolverBuilder CreateBuilder()
        {
            return new HypermediaResolverBuilderImplementation();
        }
    }

    internal class HypermediaResolverBuilderImplementation : IHypermediaResolverBuilder
    {
        private Func<IHypermediaObjectRegister> createHypermediaObjectRegister;
        private Func<IParameterSerializer> createParameterSerializer;
        private Func<IStringParser> createStringParser;
        private Func<IProblemStringReader> createProblemStringReader;
        private Func<IHypermediaObjectRegister, IStringParser, IHypermediaReader> createHypermediaReader;

        public HypermediaResolverBuilderImplementation()
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
        public IHypermediaResolverDependencies BuildDependencies()
        {
            var objectRegister = Get(this.createHypermediaObjectRegister, nameof(ConfigureObjectRegister));
            var serializer = Get(this.createParameterSerializer, nameof(WithCustomParameterSerializer));
            var stringParser = Get(this.createStringParser, nameof(WithCustomStringParser));
            var problemReader = Get(this.createProblemStringReader, nameof(WithCustomProblemStringReader));
            var reader = Get(() => this.createHypermediaReader(objectRegister, stringParser), nameof(SirenExtensions.WithSirenHypermediaReader), nameof(WithCustomHypermediaReader));
            return new ResolverDependencies(objectRegister, serializer, stringParser, problemReader, reader);
        }

        private class ResolverDependencies : IHypermediaResolverDependencies
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
        public IHypermediaResolverBuilder ConfigureObjectRegister(Action<IHypermediaObjectRegister> configure)
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
        public IHypermediaResolverBuilder WithCustomParameterSerializer(Func<IParameterSerializer> createParameterSerializer)
        {
            this.createParameterSerializer = createParameterSerializer;
            return this;
        }

        /// <summary>
        /// Internal extension point to inject an IStringParser implementation
        /// </summary>
        /// <param name="createStringParser"></param>
        /// <returns></returns>
        public IHypermediaResolverBuilder WithCustomStringParser(Func<IStringParser> createStringParser)
        {
            this.createStringParser = createStringParser;
            return this;
        }

        /// <summary>
        /// Internal extension point to inject an IProblemStringReader implementation.
        /// </summary>
        /// <param name="createProblemStringReader"></param>
        /// <returns></returns>
        public IHypermediaResolverBuilder WithCustomProblemStringReader(Func<IProblemStringReader> createProblemStringReader)
        {
            this.createProblemStringReader = createProblemStringReader;
            return this;
        }

        /// <summary>
        /// Internal extension point to inject an IHypermediaReader implementation
        /// </summary>
        /// <param name="createHypermediaReader"></param>
        /// <returns></returns>
        public IHypermediaResolverBuilder WithCustomHypermediaReader(Func<IHypermediaObjectRegister, IStringParser, IHypermediaReader> createHypermediaReader)
        {
            this.createHypermediaReader = createHypermediaReader;
            return this;
        }
    }
}