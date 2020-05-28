using System;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using NJsonSchema;
using NJsonSchema.Generation;
using WebApi.HypermediaExtensions.Util;
using WebApi.HypermediaExtensions.WebApi.RouteResolver;

#if NETSTANDARD1_6
  using Newtonsoft.Json;
#elif NETCOREAPP3_1
  using System.Text.Json;
#endif

namespace WebApi.HypermediaExtensions.JsonSchema
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

#if NETSTANDARD1_6
            return JsonSerializer.DeserializeObject(schemaData);
#else
            return JsonDocument.Parse(schemaData);
#endif
        }

        public static async Task<JsonSchema4> GenerateSchemaAsync(Type type)
        {
            var schema = await JsonSchema4.FromTypeAsync(type, s_JsonSchemaGeneratorSettings).ConfigureAwait(false);
            var keyProperties = type.GetKeyFromUriProperties();

            foreach (var keyProperty in keyProperties)
            {
                RemoveProperty(schema, keyProperty.Property.Name);
            }

            foreach (var propertyGroup in keyProperties.GroupBy(p => p.SchemaPropertyName))
            {
                var schemaPropertyName = propertyGroup.Key;
                if (schema.Properties.ContainsKey(schemaPropertyName))
                {
                    throw new JsonSchemaGenerationException($"Key property '{propertyGroup.First().Property.Name}' maps to property '{schemaPropertyName}' that already exists on type {type.BeautifulName()}");
                }

                var isRequired = propertyGroup.Any(p => p.Property.GetCustomAttribute<RequiredAttribute>() != null);
                var property = new NJsonSchema.JsonProperty { Type = JsonObjectType.String, Format = JsonFormatStrings.Uri };
                //schema factory sets minlegth of required uri properties, so do it here as well
                if (isRequired)
                    property.MinLength = 1;
                AddProperty(schema, schemaPropertyName, property, isRequired);
            }

            return schema;
        }

        static void RemoveProperty(JsonSchema4 schema, string propertyName)
        {
            schema.Properties.Remove(propertyName);
            schema.RequiredProperties.Remove(propertyName);
        }

        static void AddProperty(JsonSchema4 schema, string propertyName, NJsonSchema.JsonProperty property, bool isRequired)
        {
            schema.Properties.Add(propertyName, property);
            if (isRequired)
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
        public PropertyInfo Property { get; }
        public string SchemaPropertyName { get; }
        public string RouteTemplateParameterName { get; }
        public string ResolvedRouteTemplateParameterName => RouteTemplateParameterName ?? Property.Name;

        public KeyFromUriProperty(Type targetType, PropertyInfo property, string schemaPropertyName, string routeTemplateParameterName)
        {
            TargetType = targetType;
            Property = property;
            SchemaPropertyName = schemaPropertyName ?? property.Name;
            RouteTemplateParameterName = routeTemplateParameterName;
        }

        public override string ToString()
        {
            return $"{nameof(TargetType)}: {TargetType.BeautifulName()}, {nameof(Property)}: {Property.Name}, {nameof(RouteTemplateParameterName)}: {RouteTemplateParameterName}";
        }
    }
}