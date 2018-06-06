using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NJsonSchema;
using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.Hypermedia.Actions;
using WebApi.HypermediaExtensions.Test.Hypermedia;

namespace WebApi.HypermediaExtensions.Test.JsonSchema
{
    [TestClass]
    public class When_generating_json_schema_from_type_with_key_attribute : AsyncTestSpecification
    {
        JsonSchema4 schema;

        protected override async Task When()
        {
            schema = await JsonSchemaFactory.GenerateSchemaAsync(typeof(MyParameter)).ConfigureAwait(false);
        }

        [TestMethod]
        public void Then_key_properties_are_mapped_to_uri_schema_properties()
        {
            schema.RequiredUriPropertyShouldExist("Id");
        }

        class MyParameter : IHypermediaActionParameter
        {
            // ReSharper disable UnusedMember.Local
            [Required]
            [KeyFromUri(typeof(MyHypermediaObject))]
            public Guid Id { get; set; }
            public int SomeValue { get; set; }
            [Required]
            public Uri Uri { get; set; }
            // ReSharper restore UnusedMember.Local
        }

        class MyHypermediaObject : HypermediaObject
        {
        }
    }

    [TestClass]
    public class When_generating_json_schema_from_type_with_multiple_key_attributes : AsyncTestSpecification
    {
        JsonSchema4 schema;

        protected override async Task When()
        {
            schema = await JsonSchemaFactory.GenerateSchemaAsync(typeof(MyParameter)).ConfigureAwait(false);
        }

        [TestMethod]
        public void Then_key_properties_are_mapped_to_uri_schema_properties()
        {
            schema.RequiredUriPropertyShouldExist("UriToHmo");
            schema.RequiredUriPropertyShouldExist("UriToAnotherHmo");
            schema.Properties.Should().NotContainKeys("Id", "ParentId", "AnotherReferencedId", "AnotherReferencedIdParentId");
        }

        class MyParameter : IHypermediaActionParameter
        {
            // ReSharper disable UnusedMember.Local
            [Required]
            [KeyFromUri(typeof(MyHypermediaObject), "UriToHmo", "id")]
            public Guid Id { get; set; }
            [Required]
            [KeyFromUri(typeof(MyHypermediaObject), "UriToHmo", "parentId")]
            public Guid ParentId { get; set; }
            [Required]
            [KeyFromUri(typeof(MyHypermediaObject), "UriToAnotherHmo", "id")]
            public Guid AnotherReferencedId { get; set; }
            [Required]
            [KeyFromUri(typeof(MyHypermediaObject), "UriToAnotherHmo", "parentId")]
            public Guid AnotherReferencedIdParentId { get; set; }
            public int SomeValue { get; set; }
            [Required]
            public Uri Uri { get; set; }
            // ReSharper restore UnusedMember.Local
        }

        class MyHypermediaObject : HypermediaObject
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

    public class KeyFromUriAttribute : Attribute
    {
        public string SchemaProperyName { get; }
        public string RouteTemplateParameterName { get; }
        public Type ReferencedHypermediaObjectType { get; }

        public KeyFromUriAttribute(Type referencedHypermediaObjectType)
        {
            ReferencedHypermediaObjectType = referencedHypermediaObjectType;
        }

        public KeyFromUriAttribute(Type referencedHypermediaObjectType, string schemaProperyName,
            string routeTemplateParameterName) : this(referencedHypermediaObjectType)
        {
            SchemaProperyName = schemaProperyName;
            RouteTemplateParameterName = routeTemplateParameterName;
        }
    }
}
