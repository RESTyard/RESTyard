using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Bluehands.Hypermedia.Client.ParameterSerializer;

namespace Bluehands.Hypermedia.Client.Extensions.SystemTextJsonStringParser
{
    public class SingleSystemTextJsonObjectParameterSerializer : IParameterSerializer
    {
        private readonly JsonWriterOptions options;

        public SingleSystemTextJsonObjectParameterSerializer(JsonWriterOptions options = default)
        {
            this.options = options;
        }

        public string SerializeParameterObject(string parameterObjectName, object parameterObject)
        {
            using (var memoryStream = new MemoryStream())
            using (var writer = new Utf8JsonWriter(memoryStream, this.options))
            {
                writer.WriteStartArray();
                
                writer.WriteStartObject();
                writer.WritePropertyName(parameterObjectName);
                JsonSerializer.Serialize(writer, parameterObject);
                writer.WriteEndObject();

                writer.WriteEndArray();
                writer.Flush();

                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }
    }
}