using Bluehands.Hypermedia.Client.ParameterSerializer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bluehands.Hypermedia.Client.Extensions.NewtonsoftJson
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