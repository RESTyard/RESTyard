using System;
using Newtonsoft.Json;
using RESTyard.Client.Builder;

namespace RESTyard.Client.Extensions.NewtonsoftJson
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
        [Obsolete("Use " + nameof(WithNewtonsoftJsonObjectParameterSerializer) + " instead. The legacy array-wrapper format is no longer required by the server.")]
        public static IHypermediaResolverBuilder WithSingleNewtonsoftJsonObjectParameterSerializer(this IHypermediaResolverBuilder builder, Formatting formatting = Formatting.None)
        {
#pragma warning disable CS0618 // intentionally constructing the obsolete serializer for the obsolete builder method
            return builder.WithCustomParameterSerializer(() => new SingleNewtonsoftJsonObjectParameterSerializer(formatting));
#pragma warning restore CS0618
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
