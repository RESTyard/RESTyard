namespace Hypermedia.Client.ParameterSerializer
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class JsonObjectParameterSerializer : IParameterSerializer
    {
        private readonly Formatting formatting;

        public JsonObjectParameterSerializer(Formatting formatting = Formatting.None)
        {
            this.formatting = formatting;
        }

        public string SerializeParameterObject(string parameterObjectName, object parameterObject)
        {
            return JObject.FromObject(parameterObject).ToString(this.formatting);
        }
    }
}