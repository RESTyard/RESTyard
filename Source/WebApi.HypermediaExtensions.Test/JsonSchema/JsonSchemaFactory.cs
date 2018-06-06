using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NJsonSchema;
using NJsonSchema.Generation;
using WebApi.HypermediaExtensions.Util;

namespace WebApi.HypermediaExtensions.Test.JsonSchema
{
    public static class JsonSchemaFactory
    {
        static readonly JsonSchemaGeneratorSettings s_JsonSchemaGeneratorSettings = new JsonSchemaGeneratorSettings
        {
            FlattenInheritanceHierarchy = true,
            DefaultEnumHandling = EnumHandling.String,

        };

        public static async Task<object> Generate(Type type)
        {
            var schema = await GenerateSchemaAsync(type).ConfigureAwait(false);
            var schemaData = schema.ToJson();
            return JsonConvert.DeserializeObject(schemaData);
        }

        public static async Task<JsonSchema4> GenerateSchemaAsync(Type type)
        {
            var schema = await JsonSchema4.FromTypeAsync(type, s_JsonSchemaGeneratorSettings).ConfigureAwait(false);
            var keyProperties = type.GetKeyFromUriProperties();

            foreach (var keyProperty in keyProperties)
            {
                RemoveProperty(schema, keyProperty.PropertyInfo.Name);
            }

            foreach (var propertyGroup in keyProperties.GroupBy(p => p.SchemaPropertyName))
            {
                var schemaPropertyName = propertyGroup.Key;
                if (schema.Properties.ContainsKey(schemaPropertyName))
                {
                    throw new JsonSchemaGenerationException($"Key property '{propertyGroup.First().PropertyInfo.Name}' maps to property '{schemaPropertyName}' that already exists on type {type.BeautifulName()}");
                }

                var property = new JsonProperty { Type = JsonObjectType.String, Format = JsonFormatStrings.Uri, MinLength = 1 };
                AddRequiredProperty(schema, schemaPropertyName, property);
            }

            return schema;
        }

        static void RemoveProperty(JsonSchema4 schema, string propertyName)
        {
            schema.Properties.Remove(propertyName);
            schema.RequiredProperties.Remove(propertyName);
        }

        static void AddRequiredProperty(JsonSchema4 schema, string propertyName, JsonProperty property)
        {
            schema.Properties.Add(propertyName, property);
            schema.RequiredProperties.Add(propertyName);
        }

        public class JsonSchemaGenerationException : Exception
        {
            public JsonSchemaGenerationException(string message) : base(message)
            {
            }
        }
    }

    public static class KeyFromUriExtension
    {
        public static ImmutableArray<KeyFromUriProperty> GetKeyFromUriProperties(this Type type)
        {
            return type.GetTypeInfo()
                .GetProperties()
                .Select(p => new { p, att = p.GetCustomAttribute<KeyFromUriAttribute>() })
                .Where(p => p.att != null)
                .Select(_ => new KeyFromUriProperty(_.att.ReferencedHypermediaObjectType, _.p, _.att.SchemaProperyName, _.att.RouteTemplateParameterName))
                .ToImmutableArray();
        }
    }

    public class KeyFromUriProperty
    {
        public Type TargetType { get; }
        public PropertyInfo PropertyInfo { get; }
        public string SchemaPropertyName { get; }
        public string RouteTemplateParameterName { get; }

        public KeyFromUriProperty(Type targetType, PropertyInfo propertyInfo, string schemaPropertyName, string routeTemplateParameterName)
        {
            TargetType = targetType;
            PropertyInfo = propertyInfo;
            SchemaPropertyName = schemaPropertyName ?? propertyInfo.Name;
            RouteTemplateParameterName = routeTemplateParameterName;
        }
    }
}