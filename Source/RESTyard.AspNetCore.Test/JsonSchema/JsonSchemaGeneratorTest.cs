using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using NJsonSchema;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.AspNetCore.JsonSchema;
using RESTyard.AspNetCore.Test.Helpers;
using RESTyard.AspNetCore.Test.Hypermedia;

namespace RESTyard.AspNetCore.Test.JsonSchema
{
    [TestClass]
    public class When_generating_json_schema_from_type_with_key_attribute : AsyncTestSpecification
    {
        NJsonSchema.JsonSchema schema;

        protected override Task When()
        {
            schema = JsonSchemaFactory.GenerateSchemaAsync(typeof(MyParameter));
            return Task.CompletedTask;
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
            public Uri Id { get; set; }

            public int SomeValue { get; set; }

            //[Required]
            public Uri Uri { get; set; }
            // ReSharper restore UnusedMember.Local
        }

        class MyHypermediaObject : IHypermediaObject
        {
        }
    }

    [TestClass]
    public class When_generating_json_schema_from_type_with_multiple_key_attributes : AsyncTestSpecification
    {
        NJsonSchema.JsonSchema schema;

        protected override Task When()
        {
            schema = JsonSchemaFactory.GenerateSchemaAsync(typeof(MyParameter));
            return Task.CompletedTask;
        }

        [TestMethod]
        public void Then_key_properties_are_mapped_to_uri_schema_properties()
        {
            schema.RequiredUriPropertyShouldExist("UriToHmo");
            schema.RequiredUriPropertyShouldExist("UriToAnotherHmo");
        }

        class MyParameter : IHypermediaActionParameter
        {
            // ReSharper disable UnusedMember.Local
            [Required]
            public Uri UriToHmo { get; set; }

            [Required]
            public Uri UriToAnotherHmo { get; set; }

            public int SomeValue { get; set; }

            [Required] public Uri Uri { get; set; }
            // ReSharper restore UnusedMember.Local
        }

        class MyHypermediaObject : IHypermediaObject
        {
        }
    }

    public static class SchemaAssertionExtension
    {
        public static void RequiredUriPropertyShouldExist(this NJsonSchema.JsonSchema schema, string propertyName)
        {
            var idProperty = schema.Properties.Should().ContainKey(propertyName).WhoseValue;
            schema.RequiredProperties.Should().Contain(propertyName);
            idProperty.Type.Should().Be(JsonObjectType.String);
            idProperty.Format.Should().Be(JsonFormatStrings.Uri);
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
            clientParameter = new MyClientParameter(FormattableString.Invariant($"http://mydomain.com/customers/{GrandParentId}/{WeirdUncleId}/{ParentId}/{Id}"), 3, "http://www.anothersite.com");
            var json = JsonConvert.SerializeObject(clientParameter);
            deserialized = (MyParameter)new JsonDeserializer(typeof(MyParameter)).Deserialize(json.ToStream());
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
            public int MyId { get; set; }
            [Required]
            public string MyParentId { get; set; }
            [Required]
            public long MyGrandParentId { get; set; }
            [Required]
            public double MyWeirdUncleId { get; set; }

            public int SomeValue { get; set; }
            [Required]
            public Uri Uri { get; set; }
            // ReSharper restore UnusedMember.Local
            // ReSharper restore UnusedAutoPropertyAccessor.Local
        }

        class MyHypermediaObject : IHypermediaObject
        {
        }
    }

    [TestClass]
    public class When_generating_action_schema_with_date_only_and_time_only : TestSpecification
    {
        private NJsonSchema.JsonSchema schema;
        
        public override void When()
        {
            schema = JsonSchemaFactory.GenerateSchemaAsync(typeof(MyParameter));
        }

        [TestMethod]
        public void Then_TheTypesAreMappedProperly()
        {
            var dateOnlyProperty = schema.Properties.Should().ContainKey(nameof(MyParameter.DateOnly)).WhoseValue;
            dateOnlyProperty.Type.Should().Be(JsonObjectType.String);
            dateOnlyProperty.Format.Should().Be(JsonFormatStrings.Date);
            var timeOnlyProperty = schema.Properties.Should().ContainKey(nameof(MyParameter.TimeOnly)).WhoseValue;
            timeOnlyProperty.Type.Should().Be(JsonObjectType.String);
            timeOnlyProperty.Format.Should().Be(JsonFormatStrings.Time);
        }

        public record MyParameter(DateOnly DateOnly, TimeOnly TimeOnly);
    }
}