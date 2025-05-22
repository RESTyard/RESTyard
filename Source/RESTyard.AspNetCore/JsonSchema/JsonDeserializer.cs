using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RESTyard.AspNetCore.JsonSchema
{
    public class JsonDeserializer
    {
        readonly Type type;

        public JsonDeserializer(Type type)
        {
            this.type = type;
        }

        public object? Deserialize(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                var raw = (JObject?)new JsonSerializer().Deserialize(jsonTextReader);
                return Deserialize(raw);
            }
        }

        public object? Deserialize(JObject? raw)
        {
            if (raw == null)
            {
                return null;
            }
            
            return raw.ToObject(type);
        }
    }
}