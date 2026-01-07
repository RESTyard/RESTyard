using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AwesomeAssertions;
using Json.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.AspNetCore.JsonSchema;
using RESTyard.AspNetCore.Test.Hypermedia;

namespace RESTyard.AspNetCore.Test.JsonSchema;

// Begin: Schema generation tests
public static class SchemaValidator
{
    public static void Validate(string expectedSchemaJson, Json.Schema.JsonSchema schema)
    {
        var expectedSchemaJObject = JObject.Parse(expectedSchemaJson);
        var expectedSchema = Json.Schema.JsonSchema.FromText(expectedSchemaJObject.ToString());
        foreach (var (propertyName, propertySchema) in expectedSchema.GetProperties())
        {
            schema.GetProperties().Should().ContainKey(propertyName, $"because property '{propertyName}' should exist");

            var property = schema.GetProperties()[propertyName];
            var localPropertySchema = property.ResolveSchema(schema);
            
            localPropertySchema.GetJsonType().Should().NotBe(null, $"because property '{propertyName}' has a type defined");
            localPropertySchema.GetJsonType().Should().Be(propertySchema.GetJsonType(), $"because property '{propertyName}' has the correct type");
        }
    }
}

[TestClass]
public class When_generating_json_schema_from_type_containing_a_list : AsyncTestSpecification
{
    Json.Schema.JsonSchema schema;

   protected override Task When()
   {
       schema = new JsonSchemaFactory().GenerateSchema(typeof(MyParameter));
       return Task.CompletedTask;
   }

   [TestMethod]
   public void Then_match_schema_properties()
   {
       const string expectedSchemaJson =
            """
            {
                "properties": {
                    "UrisToHmos": {
                        "type": "array",
                        "items": {
                            "type": "string",
                            "format": "uri"
                        }
                    }
                }
            }
            """;
       SchemaValidator.Validate(expectedSchemaJson, schema);
   }

   class MyParameter : IHypermediaActionParameter
   {
       public List<Uri> UrisToHmos { get; set; } = new();
   }

   class MyHypermediaObject : IHypermediaObject;
}

[TestClass]
public class When_generating_json_schema_from_type_containing_a_list_and_not_a_list : AsyncTestSpecification
{
    Json.Schema.JsonSchema schema;

    protected override Task When()
    {
        schema = new JsonSchemaFactory().GenerateSchema(typeof(MyParameter));
        return Task.CompletedTask;
    }

    [TestMethod]
    public void Then_match_schema_properties()
    {
        const string expectedSchemaJson =
            """
            {
                "properties": {
                    "UrisToHmos": {
                        "type": "array",
                        "items": {
                            "type": "string",
                            "format": "uri"
                        }
                    },
                    "UriToHmo": {
                        "type": "string",
                        "format": "uri"
                    }
                }
            }
            """;
        SchemaValidator.Validate(expectedSchemaJson, schema);
    }

    class MyParameter : IHypermediaActionParameter
    {
        public List<Uri> UrisToHmos { get; set; } = new();
        
        public Uri UriToHmo { get; set; } = new("");
    }

    class MyHypermediaObject : IHypermediaObject;
}

[TestClass]
public class When_generating_json_schema_from_type_containing_two_types_and_a_list_and_not_a_list : AsyncTestSpecification
{
    Json.Schema.JsonSchema schema;

    protected override Task When()
    {
        schema = new JsonSchemaFactory().GenerateSchema(typeof(MyParameter));
        return Task.CompletedTask;
    }

    [TestMethod]
    public void Then_match_schema_properties()
    {
        const string expectedSchemaJson =
            """
            {
                "properties": {
                    "UrisToHmos": {
                        "type": "array",
                        "items": {
                            "type": "string",
                            "format": "uri"
                        }
                    },
                    "UriToHmo": {
                        "type": "string",
                        "format": "uri"
                    },
                    "UriToHmo2": {
                        "type": "string",
                        "format": "uri"
                    },
                    "UrisToHmos2": {
                        "type": "array",
                        "items": {
                            "type": "string",
                            "format": "uri"
                        }
                    }
                }
            }
            """;
        SchemaValidator.Validate(expectedSchemaJson, schema);
    }

    class MyParameter : IHypermediaActionParameter
    {
        public List<Uri> UrisToHmos { get; set; } = new();
        
        public Uri UriToHmo { get; set; } = new("");
        
        public List<Uri> UrisToHmos2 { get; set; } = new();
        
        public Uri UriToHmo2 { get; set; } = new("");
    }

    class MyHypermediaObject : IHypermediaObject;
    class MyHypermediaObject2 : IHypermediaObject;
}

[TestClass]
public class When_generating_json_schema_with_same_name_for_both_a_collection_and_a_non_collection : AsyncTestSpecification
{
    Json.Schema.JsonSchema schema;

    [TestMethod]
    public void GenerateSchemaAsync_Should_Fail()
    {
        try
        {
            schema = new JsonSchemaFactory().GenerateSchema(typeof(MyParameter));
            Assert.Fail("Operation should fail since UriToHmo is used for both List and not a List.");
        }
        catch (Exception)
        {
            // test pass
        }
    }

    class MyParameter : IHypermediaActionParameter
    {
        public List<Guid> Guids { get; set; } = new();
        
        public Guid Guid { get; set; } = new();
        
        public List<Guid> Guids2 { get; set; } = new();
        
        public Guid Guid2 { get; set; } = new();
    }

    class MyHypermediaObject : IHypermediaObject;
    class MyHypermediaObject2 : IHypermediaObject;
}

[TestClass]
public class When_generating_json_schema_with_same_name_for_multiple_target_types : AsyncTestSpecification
{
    Json.Schema.JsonSchema schema;

    [TestMethod]
    public void GenerateSchemaAsync_Should_Fail()
    {
        try
        {
            schema = new JsonSchemaFactory().GenerateSchema(typeof(MyParameter));
            Assert.Fail("Operation should fail since UriToHmo is used for both target types: MyHypermediaObject and MyHypermediaObject2.");
        }
        catch (Exception)
        {
            // test pass
        }
    }

    class MyParameter : IHypermediaActionParameter
    {
        public Guid Guid1 { get; set; } = new();
        
        public Guid Guid2 { get; set; } = new();
    }

    class MyHypermediaObject : IHypermediaObject;
    class MyHypermediaObject2 : IHypermediaObject;
}
// End: Schema generation tests