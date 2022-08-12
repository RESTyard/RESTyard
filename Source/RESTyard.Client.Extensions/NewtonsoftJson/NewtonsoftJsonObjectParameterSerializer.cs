using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RESTyard.Client.ParameterSerializer;

namespace RESTyard.Client.Extensions.NewtonsoftJson
{
    public class NewtonsoftJsonObjectParameterSerializer : IParameterSerializer
    {
        private readonly Formatting formatting;

        public NewtonsoftJsonObjectParameterSerializer(Formatting formatting = Formatting.None)
        {
            this.formatting = formatting;
        }

        public string SerializeParameterObject(string parameterObjectName, object parameterObject)
        {
            return JObject.FromObject(parameterObject).ToString(this.formatting);
        }
    }
}