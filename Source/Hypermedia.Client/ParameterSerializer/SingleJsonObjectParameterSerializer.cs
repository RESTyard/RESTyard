using Newtonsoft.Json.Linq;

namespace Bluehands.Hypermedia.Client.ParameterSerializer
{
    public class SingleJsonObjectParameterSerializer : IParameterSerializer
    {
        public string SerializeParameterObject(string parameterObjectName, object parameterObject)
        {
            var result = new JArray();
            var containerObject = new JObject();
            
            containerObject.Add(new JProperty(parameterObjectName, JObject.FromObject(parameterObject)));
            result.Add(containerObject);

            var resultString = result.ToString();
            return resultString;
        }
    }
}