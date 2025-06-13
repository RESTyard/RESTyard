using System;
using NJsonSchema.Generation;
using System.Text.Json;
using Newtonsoft.Json.Converters;

namespace RESTyard.AspNetCore.JsonSchema
{
    public static class JsonSchemaFactory
    {
        static readonly JsonSchemaGeneratorSettings JsonSchemaGeneratorSettings = new JsonSchemaGeneratorSettings
        {
            FlattenInheritanceHierarchy = true,
            SerializerSettings = new()
            {
                Converters =
                {
                    new StringEnumConverter(),
                },
            },
        };

        public static object Generate(Type type)

        {
            var schema = GenerateSchemaAsync(type);
            var schemaData = schema.ToJson();

            return JsonDocument.Parse(schemaData);
        }

        public static NJsonSchema.JsonSchema GenerateSchemaAsync(Type type)
        {
            var schema = NJsonSchema.JsonSchema.FromType(type, JsonSchemaGeneratorSettings);

            return schema;
        }
    }
}