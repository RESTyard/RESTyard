using System.Text.Json;
using Bluehands.Hypermedia.Client.ParameterSerializer;

namespace Bluehands.Hypermedia.Client.Extensions.SystemTextJson
{
    public class SystemTextJsonObjectParameterSerializer : IParameterSerializer
    {
        private readonly JsonSerializerOptions options;

        public SystemTextJsonObjectParameterSerializer(JsonSerializerOptions options = null)
        {
            this.options = options;
        }

        public string SerializeParameterObject(string parameterObjectName, object parameterObject)
        {
            return JsonSerializer.Serialize(parameterObject, this.options);
        }
    }
}