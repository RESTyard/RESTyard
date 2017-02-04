using System;
using Newtonsoft.Json;
using NJsonSchema;

namespace CarShack.Util
{
    public static class JsonSchemaFactory
    {
        public static object Generate(Type type)
        {
            var schema = JsonSchema4.FromTypeAsync(type).Result;
            var schemaData = schema.ToJson();
            return JsonConvert.DeserializeObject(schemaData);
        }
    }
}