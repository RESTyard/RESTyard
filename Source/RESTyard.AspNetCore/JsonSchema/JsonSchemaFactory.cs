using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using NJsonSchema;
using NJsonSchema.Generation;
using RESTyard.AspNetCore.Util;
using RESTyard.AspNetCore.WebApi.RouteResolver;
using System.Text.Json;

namespace RESTyard.AspNetCore.JsonSchema
{
    public static class JsonSchemaFactory
    {
        static readonly JsonSchemaGeneratorSettings JsonSchemaGeneratorSettings = new JsonSchemaGeneratorSettings
        {
            FlattenInheritanceHierarchy = true,
            DefaultEnumHandling = EnumHandling.String,
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
                var property = new JsonSchemaProperty { Type = JsonObjectType.String, Format = JsonFormatStrings.Uri };
                //schema factory sets minlegth of required uri properties, so do it here as well
                if (isRequired) {
                    property.MinLength = 1;
                }
                AddProperty(schema, schemaPropertyName, property, isRequired, propertyGroup.First().NestingPropertyNames);
            }

            return schema;
        }

        static void RemoveProperty(NJsonSchema.JsonSchema schema, string propertyName)
        {
            schema.Properties.Remove(propertyName);
            schema.RequiredProperties.Remove(propertyName);
        }

        static void AddProperty(NJsonSchema.JsonSchema schema, string propertyName, JsonSchemaProperty property, bool isRequired, ImmutableArray<string> nestingPropertyNames)
        {
            // navigate nesting
            var currentSchemaPosition = schema.Properties; 
            foreach (var nestingPropertyName in nestingPropertyNames)
            {

                var jsonSchemaProperty = currentSchemaPosition[nestingPropertyName];
                if (jsonSchemaProperty == null)
                {
                    throw new Exception($"Can not extend schema with propery: {propertyName} because can not navigate to nested property.");
                }
                
                currentSchemaPosition = jsonSchemaProperty.Properties;
            }
            
            
            currentSchemaPosition.Add(propertyName, property);
            if (isRequired)
            {
                schema.RequiredProperties.Add(propertyName);
            }
        }

        public class JsonSchemaGenerationException : Exception
        {
            public JsonSchemaGenerationException(string message) : base(message) { }
        }
    }

    public static class KeyFromUriExtension
    {
        public static ImmutableArray<KeyFromUriProperty> GetKeyFromUriProperties(this Type type)
        {
            var typesWithPotentialProperties =  GetTypeInfoWithNestedTypes(type, ImmutableArray<string>.Empty);
            var result = new List<KeyFromUriProperty>();
            foreach (var typeWithPotentialProperty in typesWithPotentialProperties)
            {
                foreach (var propertyInfo in typeWithPotentialProperty.TypeInfo.GetProperties())
                {
                    var customAttribute = propertyInfo.GetCustomAttribute<KeyFromUriAttribute>();
                    if (customAttribute != null)
                    {
                        result.Add(new KeyFromUriProperty(customAttribute.ReferencedHypermediaObjectType, propertyInfo, customAttribute.SchemaPropertyName, customAttribute.RouteTemplateParameterName, typeWithPotentialProperty.NestingPropertyNames));
                    }
                }
            }

            return result.ToImmutableArray();
        }

        private static IEnumerable<NestingInfo> GetTypeInfoWithNestedTypes(Type type, ImmutableArray<string> nestingPropertyNames)
        {
            var typeInfo = type.GetTypeInfo();
            yield return new NestingInfo(typeInfo, nestingPropertyNames);

            foreach (var propertyInfo in typeInfo.GetProperties())
            {
                if (propertyInfo.PropertyType.IsValueType)
                {
                    continue;
                }

                var nestedTypes = GetTypeInfoWithNestedTypes(propertyInfo.PropertyType, nestingPropertyNames.Add(propertyInfo.Name));
                foreach (var nestedType in nestedTypes)
                {
                    yield return nestedType;
                }
            }
        }

        private class NestingInfo
        {
            public NestingInfo(TypeInfo typeInfo, ImmutableArray<string> nestingPropertyNames)
            {
                NestingPropertyNames = nestingPropertyNames;
                TypeInfo = typeInfo;
            }

            public TypeInfo TypeInfo { get; }
            public ImmutableArray<string> NestingPropertyNames { get; }
        }
    }


    public class KeyFromUriProperty
    {
        public Type TargetType { get; }
        public PropertyInfo Property { get; }
        public string SchemaPropertyName { get; }
        public string RouteTemplateParameterName { get; }
        public ImmutableArray<string> NestingPropertyNames { get; }
        public string ResolvedRouteTemplateParameterName => RouteTemplateParameterName ?? Property.Name;

        public KeyFromUriProperty(Type targetType, PropertyInfo property, string schemaPropertyName, string routeTemplateParameterName, ImmutableArray<string> nestingPropertyNames)
        {
            TargetType = targetType;
            Property = property;
            SchemaPropertyName = schemaPropertyName ?? property.Name;
            RouteTemplateParameterName = routeTemplateParameterName;
            NestingPropertyNames = nestingPropertyNames;
        }

        public override string ToString()
        {
            return $"{nameof(TargetType)}: {TargetType.BeautifulName()}, {nameof(Property)}: {Property.Name}, {nameof(RouteTemplateParameterName)}: {RouteTemplateParameterName}";
        }
    }
}