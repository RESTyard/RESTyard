using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using NJsonSchema;
using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.Hypermedia.Actions;
using WebApi.HypermediaExtensions.Test.Helpers;
using WebApi.HypermediaExtensions.Test.Hypermedia;
using static System.FormattableString;

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
            schema.Properties.Should()
                .NotContainKeys("Id", "ParentId", "AnotherReferencedId", "AnotherReferencedIdParentId");
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

            [Required] public Uri Uri { get; set; }
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

    [TestClass]
    public class When_deserializing_a_parameter_with_keyfromrui_attribute : TestSpecification
    {
        MyParameter deserialized;
        MyClientParameter clientParameter;
        const string ObjectId = "becfbe6d-c453-4c3b-a396-554c30d5191d";

        public override void When()
        {
            clientParameter = new MyClientParameter($"http://mydomain.com/customers/{ObjectId}", 3, "http://www.anothersite.com");
            var json = JsonConvert.SerializeObject(clientParameter);
            deserialized = (MyParameter)new JsonDeserializer(typeof(MyParameter), "customers/{Id}").Deserialize(json.ToStream());
        }

        [TestMethod]
        public void Then_the_objects_key_is_extracted_from_the_uri()
        {
            deserialized.Id.Should().Be(ObjectId);
        }

        [TestMethod]
        public void Then_all_other_properties_are_deserialized_correctly()
        {
            deserialized.SomeValue.Should().Be(clientParameter.SomeValue);
            deserialized.Uri.Should().Be(clientParameter.Uri);
        }

        class MyClientParameter
        {
            // ReSharper disable UnusedAutoPropertyAccessor.Local
            public string Id { get; }
            public int SomeValue { get; }
            public string Uri { get; }
            // ReSharper restore UnusedAutoPropertyAccessor.Local

            public MyClientParameter(string id, int someValue, string uri)
            {
                Id = id;
                SomeValue = someValue;
                Uri = uri;
            }
        }

        class MyParameter : IHypermediaActionParameter
        {
            // ReSharper disable UnusedMember.Local
            // ReSharper disable UnusedAutoPropertyAccessor.Local
            [Required]
            [KeyFromUri(typeof(MyHypermediaObject))]
            public Guid Id { get; set; }
            
            public int SomeValue { get; set; }
            [Required]
            public Uri Uri { get; set; }
            // ReSharper restore UnusedMember.Local
            // ReSharper restore UnusedAutoPropertyAccessor.Local
        }

        class MyHypermediaObject : HypermediaObject
        {
        }
    }

    [TestClass]
    public class When_deserializing_a_parameter_composite_keys_of_different_types : TestSpecification
    {
        MyParameter deserialized;
        MyClientParameter clientParameter;
        const string ParentId = "myParentId";
        const int Id = 42;
        const long GrandParentId = 23;
        const double WeirdUncleId = 23.42;

        public override void When()
        {
            clientParameter = new MyClientParameter(Invariant($"http://mydomain.com/customers/{GrandParentId}/{WeirdUncleId}/{ParentId}/{Id}"), 3, "http://www.anothersite.com");
            var json = JsonConvert.SerializeObject(clientParameter);
            deserialized = (MyParameter)new JsonDeserializer(typeof(MyParameter), "customers/{grandParentId}/{weirdUncleId}/{parentId}/{id}").Deserialize(json.ToStream());
        }

        [TestMethod]
        public void Then_the_objects_key_is_extracted_from_the_uri()
        {
            deserialized.MyId.Should().Be(Id);
            deserialized.MyParentId.Should().Be(ParentId);
            deserialized.MyGrandParentId.Should().Be(GrandParentId);
            deserialized.MyWeirdUncleId.Should().Be(WeirdUncleId);
        }

        [TestMethod]
        public void Then_all_other_properties_are_deserialized_correctly()
        {
            deserialized.SomeValue.Should().Be(clientParameter.SomeValue);
            deserialized.Uri.Should().Be(clientParameter.Uri);
        }

        class MyClientParameter
        {
            // ReSharper disable UnusedAutoPropertyAccessor.Local
            public string UriToHmo { get; }
            public int SomeValue { get; }
            public string Uri { get; }
            // ReSharper restore UnusedAutoPropertyAccessor.Local

            public MyClientParameter(string uriToHmo, int someValue, string uri)
            {
                UriToHmo = uriToHmo;
                SomeValue = someValue;
                Uri = uri;
            }
        }

        class MyParameter : IHypermediaActionParameter
        {
            // ReSharper disable UnusedMember.Local
            // ReSharper disable UnusedAutoPropertyAccessor.Local
            [Required]
            [KeyFromUri(typeof(MyHypermediaObject), "UriToHmo", "id")]
            public int MyId { get; set; }
            [Required]
            [KeyFromUri(typeof(MyHypermediaObject), "UriToHmo", "parentId")]
            public string MyParentId { get; set; }
            [Required]
            [KeyFromUri(typeof(MyHypermediaObject), "UriToHmo", "grandParentId")]
            public long MyGrandParentId { get; set; }
            [Required]
            [KeyFromUri(typeof(MyHypermediaObject), "UriToHmo", "weirdUncleId")]
            public double MyWeirdUncleId { get; set; }

            public int SomeValue { get; set; }
            [Required]
            public Uri Uri { get; set; }
            // ReSharper restore UnusedMember.Local
            // ReSharper restore UnusedAutoPropertyAccessor.Local
        }

        class MyHypermediaObject : HypermediaObject
        {
        }
    }
}