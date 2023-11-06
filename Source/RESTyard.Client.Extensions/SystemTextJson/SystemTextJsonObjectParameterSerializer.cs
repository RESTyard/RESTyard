using System;
using System.Text.Json;
using RESTyard.Client.ParameterSerializer;

namespace RESTyard.Client.Extensions.SystemTextJson
{
    public class SystemTextJsonObjectParameterSerializer : IParameterSerializer
    {
        private readonly JsonSerializerOptions? options;

        public SystemTextJsonObjectParameterSerializer(JsonSerializerOptions? options = null)
        {
            this.options = options;
        }

        public string SerializeParameterObject(string parameterObjectName, object parameterObject)
        {
            return JsonSerializer.Serialize(parameterObject, this.options);
        }
    }
}