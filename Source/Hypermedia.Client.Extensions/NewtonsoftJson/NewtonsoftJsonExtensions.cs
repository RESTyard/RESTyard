using System;
using Bluehands.Hypermedia.Client.Builder;
using Newtonsoft.Json;

namespace Bluehands.Hypermedia.Client.Extensions.NewtonsoftJson
{
    public static class NewtonsoftJsonExtensions
    {
        /// <summary>
        /// Incoming JSON strings will be parsed using the Newtonsoft.Json library
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IHypermediaResolverBuilder WithNewtonsoftJsonStringParser(this IHypermediaResolverBuilder builder)
        {
            return builder.WithCustomStringParser(() => new NewtonsoftJsonStringParser());
        }

        /// <summary>
        /// Outgoing objects will be serialized to JSON using the Newtonsoft.Json library
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="formatting"></param>
        /// <returns></returns>
        public static IHypermediaResolverBuilder WithNewtonsoftJsonObjectParameterSerializer(this IHypermediaResolverBuilder builder, Formatting formatting = Formatting.None)
        {
            return builder.WithCustomParameterSerializer(() => new NewtonsoftJsonObjectParameterSerializer(formatting));
        }


        /// <summary>
        /// Outgoing objects will be serialized into a JSON wrapper object using the Newtonsoft.Json library
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="formatting"></param>
        /// <returns></returns>
        public static IHypermediaResolverBuilder WithSingleNewtonsoftJsonObjectParameterSerializer(this IHypermediaResolverBuilder builder, Formatting formatting = Formatting.None)
        {
            return builder.WithCustomParameterSerializer(() => new SingleNewtonsoftJsonObjectParameterSerializer(formatting));
        }

        /// <summary>
        /// Incoming problem-JSON strings will be parsed using the Newtonsoft.JSON library
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IHypermediaResolverBuilder WithNewtonsoftJsonProblemReader(this IHypermediaResolverBuilder builder)
        {
            return builder.WithCustomProblemStringReader(() => new NewtonsoftJsonProblemStringReader());
        }
    }
}
