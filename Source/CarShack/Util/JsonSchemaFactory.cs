using System;
using Newtonsoft.Json;
using NJsonSchema;
using NJsonSchema.Generation;

namespace CarShack.Util
{
    public static class JsonSchemaFactory
    {
        private static readonly JsonSchemaGeneratorSettings jsonSchemaGeneratorSettings = new JsonSchemaGeneratorSettings
        {
            FlattenInheritanceHierarchy = true,
            DefaultEnumHandling = EnumHandling.String
        };

        public static object Generate(Type type)
        {
            var schema = JsonSchema4.FromTypeAsync(type, jsonSchemaGeneratorSettings).Result;
            var schemaData = schema.ToJson();
            return JsonConvert.DeserializeObject(schemaData);
        }
    }
}