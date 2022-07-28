using System;
using System.Text.Json;
using RESTyard.Client.Builder;

namespace RESTyard.Client.Extensions.SystemTextJson
{
    public static class SystemTextJsonExtensions
    {
        /// <summary>
        /// Incoming JSON strings will be parsed using the System.Text.Json library
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IHypermediaResolverBuilder WithSystemTextJsonStringParser(this IHypermediaResolverBuilder builder)
        {
            return builder.WithCustomStringParser(() => new SystemTextJsonStringParser());
        }

        /// <summary>
        /// Outgoing objects will be serialized to JSON using the System.Text.Json library
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IHypermediaResolverBuilder WithSystemTextJsonObjectParameterSerializer(this IHypermediaResolverBuilder builder, JsonSerializerOptions options = null)
        {
            return builder.WithCustomParameterSerializer(() => new SystemTextJsonObjectParameterSerializer(options));
        }

        /// <summary>
        /// Outgoing objects will be serialized into a JSON wrapper object using the System.Text.Json library
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IHypermediaResolverBuilder WithSingleSystemTextJsonObjectParameterSerializer(this IHypermediaResolverBuilder builder, JsonWriterOptions options = default)
        {
            return builder.WithCustomParameterSerializer(() => new SingleSystemTextJsonObjectParameterSerializer(options));
        }

        /// <summary>
        /// Incoming problem-JSON strings will be parsed using the System.Text.Json library
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IHypermediaResolverBuilder WithSystemTextJsonProblemReader(this IHypermediaResolverBuilder builder)
        {
            return builder.WithCustomProblemStringReader(() => new SystemTextJsonProblemStringReader());
        }
    }
}
