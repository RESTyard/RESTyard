using System;
using System.Text.Json;
using Json.Schema;
using Json.Schema.Generation;

namespace RESTyard.AspNetCore.JsonSchema
{
    public static class JsonSchemaFactory
    {
        public static object Generate(Type type)
        {
            var schema = GenerateSchema(type);
            return JsonSerializer.SerializeToDocument(schema);
        }

        public static Json.Schema.JsonSchema GenerateSchema(Type type)
        {
            return new JsonSchemaBuilder()
                .Schema(MetaSchemas.Draft202012Id)
                .FromType(type)
                .Build();
        }
    }
}