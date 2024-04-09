﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.AspNetCore.JsonSchema;
using RESTyard.AspNetCore.Test.Hypermedia;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace RESTyard.AspNetCore.Test.JsonSchema;

public static class SchemaValidator
{
    public static void Validate(string expectedSchemaJson, NJsonSchema.JsonSchema schema)
    {
        var expectedSchemaJObject = JObject.Parse(expectedSchemaJson);
        var expectedSchema = NJsonSchema.JsonSchema.FromJsonAsync(expectedSchemaJObject.ToString()).GetAwaiter().GetResult();
        foreach (var (propertyName, propertySchema) in expectedSchema.Properties)
        {
            schema.Properties.Should().ContainKey(propertyName, $"because property '{propertyName}' should exist");
            schema.Properties[propertyName].Type.Should().NotBe(null, $"because property '{propertyName}' has a type defined");
            schema.Properties[propertyName].Type.Should().Be(propertySchema.Type, $"because property '{propertyName}' has the correct type");
        }
    }
}

[TestClass]
public class When_generating_json_schema_from_type_containing_a_list : AsyncTestSpecification
{
   NJsonSchema.JsonSchema schema;

   protected override Task When()
   {
       schema = JsonSchemaFactory.GenerateSchemaAsync(typeof(MyParameter));
       return Task.CompletedTask;
   }

   [TestMethod]
   public void Then_match_schema_properties()
   {
       const string expectedSchemaJson = @"{
            ""properties"": {
                ""UrisToHmos"": {
                    ""type"": ""array"",
                    ""items"": {
                        ""type"": ""string"",
                        ""format"": ""uri""
                    }
                }
            }
        }";
       SchemaValidator.Validate(expectedSchemaJson, schema);
   }

   class MyParameter : IHypermediaActionParameter
   {
       [KeyFromUri(typeof(MyHypermediaObject), "UrisToHmos")]
       public List<Guid> Guids { get; set; } = new();
   }

   class MyHypermediaObject : HypermediaObject;
}

[TestClass]
public class When_generating_json_schema_from_type_containing_a_list_and_not_a_list : AsyncTestSpecification
{
    NJsonSchema.JsonSchema schema;

    protected override Task When()
    {
        schema = JsonSchemaFactory.GenerateSchemaAsync(typeof(MyParameter));
        return Task.CompletedTask;
    }

    [TestMethod]
    public void Then_match_schema_properties()
    {
        const string expectedSchemaJson = @"{
            ""properties"": {
                ""UrisToHmos"": {
                    ""type"": ""array"",
                    ""items"": {
                        ""type"": ""string"",
                        ""format"": ""uri""
                    }
                },
                ""UriToHmo"": {
                    ""type"": ""string"",
                    ""format"": ""uri""
                }
            }
        }";
        SchemaValidator.Validate(expectedSchemaJson, schema);
    }

    class MyParameter : IHypermediaActionParameter
    {
        [KeyFromUri(typeof(MyHypermediaObject), "UrisToHmos")]
        public List<Guid> Guids { get; set; } = new();
        
        [KeyFromUri(typeof(MyHypermediaObject), "UriToHmo")]
        public Guid Guid { get; set; } = new();
    }

    class MyHypermediaObject : HypermediaObject;
}

[TestClass]
public class When_generating_json_schema_from_type_containing_two_types_and_a_list_and_not_a_list : AsyncTestSpecification
{
    NJsonSchema.JsonSchema schema;

    protected override Task When()
    {
        schema = JsonSchemaFactory.GenerateSchemaAsync(typeof(MyParameter));
        return Task.CompletedTask;
    }

    [TestMethod]
    public void Then_match_schema_properties()
    {
        const string expectedSchemaJson = @"{
            ""properties"": {
                ""UrisToHmos"": {
                    ""type"": ""array"",
                    ""items"": {
                        ""type"": ""string"",
                        ""format"": ""uri""
                    }
                },
                ""UriToHmo"": {
                    ""type"": ""string"",
                    ""format"": ""uri""
                },
                ""UriToHmo2"": {
                    ""type"": ""string"",
                    ""format"": ""uri""
                },
                ""UrisToHmos2"": {
                    ""type"": ""array"",
                    ""items"": {
                        ""type"": ""string"",
                        ""format"": ""uri""
                    }
                }
            }
        }";
        SchemaValidator.Validate(expectedSchemaJson, schema);
    }

    class MyParameter : IHypermediaActionParameter
    {
        [KeyFromUri(typeof(MyHypermediaObject), "UrisToHmos")]
        public List<Guid> Guids { get; set; } = new();
        
        [KeyFromUri(typeof(MyHypermediaObject), "UriToHmo")]
        public Guid Guid { get; set; } = new();
        
        [KeyFromUri(typeof(MyHypermediaObject2), "UrisToHmos2")]
        public List<Guid> Guids2 { get; set; } = new();
        
        [KeyFromUri(typeof(MyHypermediaObject2), "UriToHmo2")]
        public Guid Guid2 { get; set; } = new();
    }

    class MyHypermediaObject : HypermediaObject;
    class MyHypermediaObject2 : HypermediaObject;
}

[TestClass]
public class When_generating_json_schema_with_same_name_for_both_a_collection_and_a_non_collection : AsyncTestSpecification
{
    NJsonSchema.JsonSchema schema;

    [TestMethod]
    public void GenerateSchemaAsync_Should_Fail()
    {
        try
        {
            schema = JsonSchemaFactory.GenerateSchemaAsync(typeof(MyParameter));
            Assert.Fail("Operation should fail since UriToHmo is used for both List and not a List.");
        }
        catch (Exception)
        {
            // test pass
        }
    }

    class MyParameter : IHypermediaActionParameter
    {
        [KeyFromUri(typeof(MyHypermediaObject), "UriToHmo")]
        public List<Guid> Guids { get; set; } = new();
        
        [KeyFromUri(typeof(MyHypermediaObject), "UriToHmo")]
        public Guid Guid { get; set; } = new();
        
        [KeyFromUri(typeof(MyHypermediaObject2), "UrisToHmos2")]
        public List<Guid> Guids2 { get; set; } = new();
        
        [KeyFromUri(typeof(MyHypermediaObject2), "UriToHmo2")]
        public Guid Guid2 { get; set; } = new();
    }

    class MyHypermediaObject : HypermediaObject;
    class MyHypermediaObject2 : HypermediaObject;
}

[TestClass]
public class When_generating_json_schema_with_same_name_for_multiple_target_types : AsyncTestSpecification
{
    NJsonSchema.JsonSchema schema;

    [TestMethod]
    public void GenerateSchemaAsync_Should_Fail()
    {
        try
        {
            schema = JsonSchemaFactory.GenerateSchemaAsync(typeof(MyParameter));
            Assert.Fail("Operation should fail since UriToHmo is used for both target types: MyHypermediaObject and MyHypermediaObject2.");
        }
        catch (Exception)
        {
            // test pass
        }
    }

    class MyParameter : IHypermediaActionParameter
    {
        [KeyFromUri(typeof(MyHypermediaObject), "UriToHmo")]
        public Guid Guid1 { get; set; } = new();
        
        [KeyFromUri(typeof(MyHypermediaObject2), "UriToHmo")]
        public Guid Guid2 { get; set; } = new();
    }

    class MyHypermediaObject : HypermediaObject;
    class MyHypermediaObject2 : HypermediaObject;
}