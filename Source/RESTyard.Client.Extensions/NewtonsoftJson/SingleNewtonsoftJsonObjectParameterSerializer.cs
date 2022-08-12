using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RESTyard.Client.ParameterSerializer;

namespace RESTyard.Client.Extensions.NewtonsoftJson
{
    public class SingleNewtonsoftJsonObjectParameterSerializer : IParameterSerializer
    {
        private readonly Formatting formatting;

        public SingleNewtonsoftJsonObjectParameterSerializer(Formatting formatting = Formatting.None)
        {
            this.formatting = formatting;
        }

        public string SerializeParameterObject(string parameterObjectName, object parameterObject)
        {
            var result = new JArray();
            var containerObject = new JObject
            {
                new JProperty(parameterObjectName, JObject.FromObject(parameterObject))
            };

            result.Add(containerObject);

            var resultString = result.ToString(this.formatting);
            return resultString;
        }
    }
}