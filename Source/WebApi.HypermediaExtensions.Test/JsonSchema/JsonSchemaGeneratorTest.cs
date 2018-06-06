using System;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using NJsonSchema;
using NJsonSchema.Generation;
using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.Hypermedia.Actions;
using WebApi.HypermediaExtensions.Test.Hypermedia;
using WebApi.HypermediaExtensions.Util;

namespace WebApi.HypermediaExtensions.Test.JsonSchema
{
    [TestClass]
    public class When_generating_json_schema_from_type_with_key_attribute : TestSpecification
    {
        JsonSchema4 schema;

        public override void When()
        {
            schema = JsonSchemaFactory.GenerateSchema(typeof(MyParameter));
        }

        [TestMethod]
        public void Then_key_properties_are_mapped_to_uri_schema_properties()
        {
            schema.RequiredUriPropertyShouldExist("Id");
        }

        public class MyParameter : IHypermediaActionParameter
        {
            [Required]
            [KeyFromRoute(typeof(MyHypermediaObject))]
            public Guid Id { get; set; }

            public int SomeValue { get; set; }

            [Required]
            public Uri Uri { get; set; }
        }

        public class MyHypermediaObject : HypermediaObject
        {

        }
    }

    [TestClass]
    public class When_generating_json_schema_from_type_with_multiple_key_attributes : TestSpecification
    {
        JsonSchema4 schema;

        public override void When()
        {
            schema = JsonSchemaFactory.GenerateSchema(typeof(MyParameter));
        }

        [TestMethod]
        public void Then_key_properties_are_mapped_to_uri_schema_properties()
        {
            schema.RequiredUriPropertyShouldExist("UriToHmo");
            schema.RequiredUriPropertyShouldExist("UriToAnotherHmo");
            schema.Properties.Should().NotContainKeys("Id", "ParentId", "AnotherReferencedId", "AnotherReferencedIdParentId");
        }

        public class MyParameter : IHypermediaActionParameter
        {
            [Required]
            [KeyFromRoute(typeof(MyHypermediaObject), "UriToHmo", "id")]
            public Guid Id { get; set; }

            [Required]
            [KeyFromRoute(typeof(MyHypermediaObject), "UriToHmo", "parentId")]
            public Guid ParentId { get; set; }

            [Required]
            [KeyFromRoute(typeof(MyHypermediaObject), "UriToAnotherHmo", "id")]
            public Guid AnotherReferencedId { get; set; }

            [Required]
            [KeyFromRoute(typeof(MyHypermediaObject), "UriToAnotherHmo", "parentId")]
            public Guid AnotherReferencedIdParentId { get; set; }

            public int SomeValue { get; set; }

            [Required]
            public Uri Uri { get; set; }
        }

        public class MyHypermediaObject : HypermediaObject
        {

        }
    }

    public static class SchemaAssertionExtension
    {
        public static void RequiredUriPropertyShouldExist(this JsonSchema4 schema, string propertyName)
        {
            schema.Properties.Should().ContainKey(propertyName);
            schema.RequiredProperties.Should().Contain(propertyName);
            var idProperty = schema.Properties[propertyName];
            idProperty.Type.Should().Be(JsonObjectType.String);
            idProperty.Format.Should().Be(JsonFormatStrings.Uri);
        }
    }

    public class KeyFromRouteAttribute : Attribute
    {
        public string SchemaProperyName { get; }
        public string RouteTemplateParameterName { get; }
        public Type ReferencedHypermediaObjectType { get; }

        public KeyFromRouteAttribute(Type referencedHypermediaObjectType)
        {
            ReferencedHypermediaObjectType = referencedHypermediaObjectType;
        }

        public KeyFromRouteAttribute(Type referencedHypermediaObjectType, string schemaProperyName,
            string routeTemplateParameterName) : this(referencedHypermediaObjectType)
        {
            SchemaProperyName = schemaProperyName;
            RouteTemplateParameterName = routeTemplateParameterName;
        }
    }

    public static class JsonSchemaFactory
    {
        static readonly JsonSchemaGeneratorSettings s_JsonSchemaGeneratorSettings = new JsonSchemaGeneratorSettings
        {
            FlattenInheritanceHierarchy = true,
            DefaultEnumHandling = EnumHandling.String,

        };

        public static object Generate(Type type)
        {
            var schema = GenerateSchema(type);

            var schemaData = schema.ToJson();
            return JsonConvert.DeserializeObject(schemaData);
        }

        public static JsonSchema4 GenerateSchema(Type type)
        {
            var schema = JsonSchema4.FromTypeAsync(type, s_JsonSchemaGeneratorSettings).GetAwaiter().GetResult();
            var keyProperties = type.GetTypeInfo().GetProperties()
                .Select(p => new { p, att = p.GetCustomAttribute<KeyFromRouteAttribute>() })
                .Where(p => p.att != null)
                .ToImmutableArray();

            if (keyProperties.Any())
            {
                foreach (var keyProperty in keyProperties)
                {
                    schema.Properties.Remove(keyProperty.p.Name);
                    schema.RequiredProperties.Remove(keyProperty.p.Name);
                }

                var propertiesBySchemaPropertyName = keyProperties.GroupBy(p => p.att.SchemaProperyName ?? p.p.Name);
                foreach (var propertyGroup in propertiesBySchemaPropertyName)
                {
                    var schemaPropertyName = propertyGroup.Key;
                    if (schema.Properties.ContainsKey(schemaPropertyName))
                    {
                        throw new JsonSchemaGenerationException($"Key property '{propertyGroup.First().p.Name}' maps to property '{schemaPropertyName}' that already exists on type {type.BeautifulName()}");
                    }
                    schema.Properties.Add(schemaPropertyName, new JsonProperty { Type = JsonObjectType.String, Format = JsonFormatStrings.Uri, MinLength = 1 });
                    schema.RequiredProperties.Add(schemaPropertyName);
                }
            }

            return schema;
        }

        public class JsonSchemaGenerationException : Exception
        {
            public JsonSchemaGenerationException(string message) : base(message)
            {
            }
        }
    }
}
