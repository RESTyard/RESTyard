using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace RESTyard.AspNetCore.JsonSchema
{
    public class JsonDeserializer
    {
        readonly Type type;

        public JsonDeserializer(Type type)
        {
            this.type = type;
        }

        /// <summary>
        /// Deserializes the given stream into the configured type. The provided <paramref name="options"/>
        /// must be the request's <see cref="JsonSerializerOptions"/> so DI-registered custom converters apply.
        /// </summary>
        public object? Deserialize(Stream stream, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize(stream, type, options);
        }

        /// <summary>
        /// Deserializes the given parsed JSON node into the configured type. The provided <paramref name="options"/>
        /// must be the request's <see cref="JsonSerializerOptions"/> so DI-registered custom converters apply.
        /// </summary>
        public object? Deserialize(JsonNode? node, JsonSerializerOptions options)
        {
            if (node is null)
            {
                return null;
            }

            return JsonSerializer.Deserialize(node, type, options);
        }
    }
}
