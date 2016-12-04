using System;
using Newtonsoft.Json.Schema;

namespace CarShack.Util
{
    public static class JsonSchemaFactory
    {
        public static JsonSchema Generate(Type type)
        {
            var jsonSchemaGenerator = new JsonSchemaGenerator();
            var schema = jsonSchemaGenerator.Generate(type);
            schema.Title = type.Name;
            return schema;
        }
    }
}