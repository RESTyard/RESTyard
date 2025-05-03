using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.AspNetCore.JsonSchema;
using RESTyard.AspNetCore.Test.Helpers;
using RESTyard.AspNetCore.Test.Hypermedia;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace RESTyard.AspNetCore.Test.JsonSchema;

// Begin: Schema generation tests
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

   class MyHypermediaObject : IHypermediaObject;
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

    class MyHypermediaObject : IHypermediaObject;
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

    class MyHypermediaObject : IHypermediaObject;
    class MyHypermediaObject2 : IHypermediaObject;
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

    class MyHypermediaObject : IHypermediaObject;
    class MyHypermediaObject2 : IHypermediaObject;
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

    class MyHypermediaObject : IHypermediaObject;
    class MyHypermediaObject2 : IHypermediaObject;
}
// End: Schema generation tests

// Begin: Deserialization tests
[TestClass]
public class When_deserializing_a_list_parameter_with_keyfromuri_attribute : TestSpecification
{
    MyParameter deserialized;
    MyClientParameter clientParameter;
    List<string> ObjectIds = new()
    {
        "becfbe6d-c453-4c3b-a396-554c30d5191d",
        "becfbe6d-c453-4c3b-a396-554c30d5191e",
        "becfbe6d-c453-4c3b-a396-554c30d5191f"
    };

    public override void When()
    {
        clientParameter = new MyClientParameter([$"http://mydomain.com/customers/{ObjectIds[0]}", $"http://mydomain.com/customers/{ObjectIds[1]}", $"http://mydomain.com/customers/{ObjectIds[2]}"], 3, "http://www.anothersite.com");
        var json = JsonConvert.SerializeObject(clientParameter);
        deserialized = (MyParameter)new JsonDeserializer(typeof(MyParameter), t => "customers/{Id}").Deserialize(json.ToStream());
    }

    [TestMethod]
    public void Then_the_objects_key_is_extracted_from_the_uri()
    {
        deserialized.Ids.Select(x => x.ToString()).Should().BeEquivalentTo(ObjectIds);
    }

    [TestMethod]
    public void Then_all_other_properties_are_deserialized_correctly()
    {
        deserialized.SomeValue.Should().Be(clientParameter.SomeValue);
        deserialized.Uri.Should().Be(clientParameter.Uri);
    }

    class MyClientParameter
    {
        public List<string> Ids { get; }
        public int SomeValue { get; }
        public string Uri { get; }

        public MyClientParameter(List<string> ids, int someValue, string uri)
        {
            Ids = ids;
            SomeValue = someValue;
            Uri = uri;
        }
    }

    class MyParameter : IHypermediaActionParameter
    {
        [KeyFromUri(typeof(MyHypermediaObject))]
        public List<Guid> Ids { get; set; }
        
        public int SomeValue { get; set; }
        public Uri Uri { get; set; }
    }

    class MyHypermediaObject : IHypermediaObject;
}

[TestClass]
public class When_deserializing_both_list_and_nonlist_parameters_with_keyfromuri_attribute : TestSpecification
{
    MyParameter deserialized;
    MyClientParameter clientParameter;
    List<string> ObjectIds = new()
    {
        "becfbe6d-c453-4c3b-a396-554c30d5191d",
        "becfbe6d-c453-4c3b-a396-554c30d5191e",
        "becfbe6d-c453-4c3b-a396-554c30d5191f"
    };

    public override void When()
    {
        clientParameter = new MyClientParameter([$"http://mydomain.com/customers/{ObjectIds[0]}", $"http://mydomain.com/customers/{ObjectIds[1]}", $"http://mydomain.com/customers/{ObjectIds[2]}"], 3, "http://www.anothersite.com", $"http://mydomain.com/customers/{ObjectIds[1]}");
        var json = JsonConvert.SerializeObject(clientParameter);
        deserialized = (MyParameter)new JsonDeserializer(typeof(MyParameter), t => "customers/{Id}").Deserialize(json.ToStream());
    }

    [TestMethod]
    public void Then_the_objects_key_is_extracted_from_the_uri()
    {
        deserialized.Ids.Select(x => x.ToString()).Should().BeEquivalentTo(ObjectIds);
        deserialized.ParentId.ToString().Should().BeEquivalentTo(ObjectIds[1]);
    }

    [TestMethod]
    public void Then_all_other_properties_are_deserialized_correctly()
    {
        deserialized.SomeValue.Should().Be(clientParameter.SomeValue);
        deserialized.Uri.Should().Be(clientParameter.Uri);
    }

    class MyClientParameter
    {
        public List<string> Ids { get; }
        public string ParentId { get; }
        public int SomeValue { get; }
        public string Uri { get; }

        public MyClientParameter(List<string> ids, int someValue, string uri, string parentId)
        {
            Ids = ids;
            SomeValue = someValue;
            Uri = uri;
            ParentId = parentId;
        }
    }

    class MyParameter : IHypermediaActionParameter
    {
        [KeyFromUri(typeof(MyHypermediaObject))]
        public List<Guid> Ids { get; set; }
        
        [KeyFromUri(typeof(MyHypermediaObject))]
        public Guid ParentId { get; set; }
        
        public int SomeValue { get; set; }
        public Uri Uri { get; set; }
    }

    class MyHypermediaObject : IHypermediaObject;
}

[TestClass]
public class When_deserializing_both_list_and_nonlist_parameters_with_keyfromuri_attribute_using_same_schema_name : AsyncTestSpecification
{
    MyParameter deserialized;
    MyClientParameter clientParameter;
    List<string> ObjectIds =
    [
        "becfbe6d-c453-4c3b-a396-554c30d5191d",
        "becfbe6d-c453-4c3b-a396-554c30d5191e",
        "becfbe6d-c453-4c3b-a396-554c30d5191f"
    ];

    [TestMethod]
    public void deserialization_should_fail()
    {
        try
        {
            clientParameter = new MyClientParameter([$"http://mydomain.com/customers/{ObjectIds[0]}", $"http://mydomain.com/customers/{ObjectIds[1]}", $"http://mydomain.com/customers/{ObjectIds[2]}"], 3, "http://www.anothersite.com", $"http://mydomain.com/customers/{ObjectIds[1]}");
            var json = JsonConvert.SerializeObject(clientParameter);
            deserialized =
                (MyParameter)new JsonDeserializer(typeof(MyParameter), t => "customers/{Id}").Deserialize(
                    json.ToStream());
            Assert.Fail("Operation should fail since uri is used for both List and not a List.");
        }
        catch (Exception)
        {
            // test pass
        }
    }

    class MyClientParameter
    {
        public List<string> Ids { get; }
        public string ParentId { get; }
        public int SomeValue { get; }
        public string Uri { get; }

        public MyClientParameter(List<string> ids, int someValue, string uri, string parentId)
        {
            Ids = ids;
            SomeValue = someValue;
            Uri = uri;
            ParentId = parentId;
        }
    }

    class MyParameter : IHypermediaActionParameter
    {
        [KeyFromUri(typeof(MyHypermediaObject), "uri")]
        public List<Guid> Ids { get; set; }
        
        [KeyFromUri(typeof(MyHypermediaObject), "uri")]
        public Guid ParentId { get; set; }
        
        public int SomeValue { get; set; }
        public Uri Uri { get; set; }
    }

    class MyHypermediaObject : IHypermediaObject;
}

[TestClass]
public class When_deserializing_multiple_types_of_parameters_with_keyfromuri_attribute : AsyncTestSpecification
{
    MyParameter deserialized;
    MyClientParameter clientParameter;
    string ObjectId = "becfbe6d-c453-4c3b-a396-554c30d5191e";
    string AnotherObjectId = "becfbe6d-c453-4c3b-a396-554c30d5191f";

    protected override Task When()
    {
        clientParameter = new MyClientParameter($"http://mydomain.com/customers/{ObjectId}", 3, "http://www.anothersite.com", $"http://mydomain.com/agents/{AnotherObjectId}");
        var json = JsonConvert.SerializeObject(clientParameter);
        deserialized =
            (MyParameter)new JsonDeserializer(typeof(MyParameter),
                t => ImmutableArray.Create(new string[]{ "customers/{Id}", "agents/{Id}" })).Deserialize(
                json.ToStream());
        return Task.CompletedTask;
    }

    [TestMethod]
    public void Then_the_objects_key_is_extracted_from_the_uri()
    {
        deserialized.Id.ToString().Should().BeEquivalentTo(ObjectId);
        deserialized.AnotherId.ToString().Should().BeEquivalentTo(AnotherObjectId);
    }

    [TestMethod]
    public void Then_all_other_properties_are_deserialized_correctly()
    {
        deserialized.SomeValue.Should().Be(clientParameter.SomeValue);
        deserialized.Uri.Should().Be(clientParameter.Uri);
    }
    
    class MyClientParameter
    {
        public string Id { get; }
        public string AnotherId { get; }
        public int SomeValue { get; }
        public string Uri { get; }

        public MyClientParameter(string id, int someValue, string uri, string anotherId)
        {
            Id = id;
            SomeValue = someValue;
            Uri = uri;
            AnotherId = anotherId;
        }
    }

    class MyParameter : IHypermediaActionParameter
    {
        [KeyFromUri(typeof(MyHypermediaObject))]
        public Guid Id { get; set; }
        
        [KeyFromUri(typeof(MyHypermediaObject2))]
        public Guid AnotherId { get; set; }
        
        public int SomeValue { get; set; }
        public Uri Uri { get; set; }
    }

    class MyHypermediaObject : IHypermediaObject;
    class MyHypermediaObject2 : IHypermediaObject;
}


[TestClass]
public class When_deserializing_multiple_types_of_parameters_with_keyfromuri_attribute_using_same_schema_name : AsyncTestSpecification
{
    MyParameter deserialized;
    MyClientParameter clientParameter;
    string ObjectId = "becfbe6d-c453-4c3b-a396-554c30d5191e";
    string AnotherObjectId = "becfbe6d-c453-4c3b-a396-554c30d5191f";

    [TestMethod]
    public void deserialization_should_fail()
    {
        try
        {
            clientParameter = new MyClientParameter($"http://mydomain.com/customers/{ObjectId}", 3, "http://www.anothersite.com", $"http://mydomain.com/agents/{AnotherObjectId}");
            var json = JsonConvert.SerializeObject(clientParameter);
            deserialized =
                (MyParameter)new JsonDeserializer(typeof(MyParameter),
                    t => ImmutableArray.Create(new string[]{ "customers/{Id}", "agents/{Id}" })).Deserialize(
                    json.ToStream());
            Assert.Fail("Operation should fail since uri is used for both MyHypermediaObject and MyHypermediaObject2.");
        }
        catch (Exception)
        {
            // test pass
        }
    }

    class MyClientParameter
    {
        public string Id { get; }
        public string AnotherId { get; }
        public int SomeValue { get; }
        public string Uri { get; }

        public MyClientParameter(string id, int someValue, string uri, string anotherId)
        {
            Id = id;
            SomeValue = someValue;
            Uri = uri;
            AnotherId = anotherId;
        }
    }

    class MyParameter : IHypermediaActionParameter
    {
        [KeyFromUri(typeof(MyHypermediaObject), "uri")]
        public Guid Id { get; set; }
        
        [KeyFromUri(typeof(MyHypermediaObject2), "uri")]
        public Guid AnotherId { get; set; }
        
        public int SomeValue { get; set; }
        public Uri Uri { get; set; }
    }

    class MyHypermediaObject : IHypermediaObject;
    class MyHypermediaObject2 : IHypermediaObject;
}

// End: Deserialization tests