using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AwesomeAssertions;
using Json.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
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
        Json.Schema.JsonSchema schema;

        protected override Task When()
        {
            schema = new JsonSchemaFactory().GenerateSchema(typeof(MyParameter));
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
            public required Uri Id { get; set; }

            public int SomeValue { get; set; }

            [Required]
            public required Uri Uri { get; set; }
            // ReSharper restore UnusedMember.Local
        }

        class MyHypermediaObject : IHypermediaObject
        {
        }
    }

    [TestClass]
    public class When_generating_json_schema_from_type_with_multiple_key_attributes : AsyncTestSpecification
    {
        Json.Schema.JsonSchema schema;

        protected override Task When()
        {
            schema = new JsonSchemaFactory().GenerateSchema(typeof(MyParameter));
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
            public required Uri UriToHmo { get; set; }

            public  required Uri UriToAnotherHmo { get; set; }

            public int SomeValue { get; set; }

            public  required Uri Uri { get; set; }
            // ReSharper restore UnusedMember.Local
        }

        class MyHypermediaObject : IHypermediaObject;
    }

    public static class SchemaAssertionExtension
    {
        public static void RequiredUriPropertyShouldExist(this Json.Schema.JsonSchema schema, string propertyName)
        { 
            schema.GetRequired().Should().Contain(propertyName);
            
            var property = schema.GetProperties().Should().ContainKey(propertyName).WhoseValue;
            var resolvedSchema = property.ResolveSchema(schema);
            resolvedSchema.Should().NotBeNull("Schema must be found either inline or as ref");
            resolvedSchema!.GetJsonType().Should().Be(SchemaValueType.String);
            resolvedSchema!.GetFormat().Should().Be(Formats.Uri);
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
        private Json.Schema.JsonSchema schema;
        
        public override void When()
        {
            schema = new JsonSchemaFactory().GenerateSchema(typeof(MyParameter));
        }

        [TestMethod]
        public void Then_TheTypesAreMappedProperly()
        {
            var dateOnlyProperty = schema.GetProperties().Should().ContainKey(nameof(MyParameter.DateOnly)).WhoseValue;
            dateOnlyProperty.GetJsonType().Should().Be(SchemaValueType.String);
            dateOnlyProperty.GetFormat().Should().Be(Formats.Date);
            var timeOnlyProperty = schema.GetProperties().Should().ContainKey(nameof(MyParameter.TimeOnly)).WhoseValue;
            timeOnlyProperty.GetJsonType().Should().Be(SchemaValueType.String);
            timeOnlyProperty.GetFormat().Should().Be(Formats.Time);
        }

        public record MyParameter(DateOnly DateOnly, TimeOnly TimeOnly);
    }
    
    [TestClass]
    public class When_generating_action_schema_with_date_time_offset_date_time_timespan : TestSpecification
    {
        private Json.Schema.JsonSchema schema;
        public override void When()
        {
            schema = new JsonSchemaFactory().GenerateSchema(typeof(MyParameterTimes));
        }

        [TestMethod]
        public void Then_TheTypesAreMappedProperly()
        {
            var dateTimeOffsetProperty = schema.GetProperties().Should().ContainKey(nameof(MyParameterTimes.DateTimeOffset)).WhoseValue;
            dateTimeOffsetProperty.GetJsonType().Should().Be(SchemaValueType.String);
            dateTimeOffsetProperty.GetFormat().Should().Be(Formats.DateTime);
            
            var dateTimeProperty = schema.GetProperties().Should().ContainKey(nameof(MyParameterTimes.DateTime)).WhoseValue;
            dateTimeProperty.GetJsonType().Should().Be(SchemaValueType.String);
            dateTimeProperty.GetFormat().Should().Be(Formats.DateTime);

            var timeSpanProperty = schema.GetProperties().Should().ContainKey(nameof(MyParameterTimes.TimeSpan)).WhoseValue;
            timeSpanProperty.GetJsonType().Should().Be(SchemaValueType.String);
            // Formats.Duration would require the serialization to be ISO 8601 Duration, so no check for now
            
            var dateTimeOffsetNullableProperty = schema.GetProperties().Should().ContainKey(nameof(MyParameterTimes.DateTimeOffsetNullable)).WhoseValue;
            dateTimeOffsetNullableProperty.GetJsonType().Should().Be(SchemaValueType.String | SchemaValueType.Null);
            dateTimeOffsetNullableProperty.GetFormat().Should().Be(Formats.DateTime);
        }

        public record MyParameterTimes(DateTimeOffset DateTimeOffset, DateTime DateTime, TimeSpan TimeSpan, DateTimeOffset? DateTimeOffsetNullable);
    }
}