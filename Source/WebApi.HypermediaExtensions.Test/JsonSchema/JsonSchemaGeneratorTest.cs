using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NJsonSchema;
using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.Hypermedia.Actions;
using WebApi.HypermediaExtensions.Test.Hypermedia;
using JsonProperty = Newtonsoft.Json.Serialization.JsonProperty;

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
            clientParameter = new MyClientParameter($"http://mydomain.com/customers/{ObjectId}", 3, "http://www.blubs.com");
            var json = JsonConvert.SerializeObject(clientParameter);
            deserialized = (MyParameter)new JsonDeserializer(typeof(MyParameter), "customers/{Id}").Deserialize(GenerateStreamFromString(json));
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

        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        class MyClientParameter
        {
            public string Id { get; }
            public int SomeValue { get; }
            public string Uri { get; }

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

    public class JsonDeserializer
    {
        readonly Type type;
        readonly TemplateMatcher templateMatcher;
        ImmutableArray<KeyFromUriProperty> keyFromUriProperties;
        readonly JsonSerializerSettings jsonSerializerSettings;

        public JsonDeserializer(Type type, string routeTemplate)
        {
            this.type = type;

            var template = TemplateParser.Parse(routeTemplate);
            templateMatcher = new TemplateMatcher(template, GetDefaults(template));

            keyFromUriProperties = type.GetKeyFromUriProperties();
            jsonSerializerSettings = new JsonSerializerSettings();

            if (keyFromUriProperties.Any())
            {
                jsonSerializerSettings.ContractResolver = new IgnorePropertiesContractResolver(keyFromUriProperties.Select(p => p.SchemaPropertyName));
            }
        }

        static RouteValueDictionary GetDefaults(RouteTemplate parsedTemplate)
        {
            var result = new RouteValueDictionary();

            foreach (var parameter in parsedTemplate.Parameters)
            {
                if (parameter.DefaultValue != null)
                {
                    result.Add(parameter.Name, parameter.DefaultValue);
                }
            }

            return result;
        }

        public object Deserialize(Stream stream)
        {
            var serializer = JsonSerializer.Create(jsonSerializerSettings);
            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                var raw = (JObject)new JsonSerializer().Deserialize(jsonTextReader);
                foreach (var propertyByUriProperty in keyFromUriProperties.GroupBy(p => p.SchemaPropertyName))
                {
                    var uri = (string)raw[propertyByUriProperty.Key];
                    raw.Remove(propertyByUriProperty.Key);
                    RouteValueDictionary values;
                    if (templateMatcher.TryGetValuesFromRequest(new Uri(uri).LocalPath, out values))
                    {
                        foreach (var keyFromUriProperty in propertyByUriProperty)
                        {
                            //TODO: parse to correct type
                            raw.Add(new JProperty(keyFromUriProperty.PropertyInfo.Name, (string)values[keyFromUriProperty.RouteTemplateParameterName ?? keyFromUriProperty.PropertyInfo.Name]));
                        }
                    }
                }

                var json = raw.ToString();
                return raw.ToObject(type);
            }
        }

        class IgnorePropertiesContractResolver : DefaultContractResolver
        {
            readonly ImmutableHashSet<string> propertiesToIgnore;

            public IgnorePropertiesContractResolver(IEnumerable<string> propertiesToIgnore)
            {
                this.propertiesToIgnore = propertiesToIgnore.ToImmutableHashSet();
            }

            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                var properties = base.CreateProperties(type, memberSerialization);
                return properties.Where(p => !propertiesToIgnore.Contains(p.PropertyName)).ToList();
            }
        }

        
    }

    public static class RouteMatcher
        {
            public static bool TryMatch(string routeTemplate, Uri requestPath, out RouteValueDictionary values)
            {
                var template = TemplateParser.Parse(routeTemplate);
                var matcher = new TemplateMatcher(template, GetDefaults(template));

                var requestLocalPath = requestPath.LocalPath;
                return TryGetValuesFromRequest(matcher, requestLocalPath, out values);
            }

            public static bool TryGetValuesFromRequest(this TemplateMatcher matcher, string requestLocalPath, out RouteValueDictionary values)
            {
                values = new RouteValueDictionary();
                return matcher.TryMatch(requestLocalPath, values);
            }

            // This method extracts the default argument values from the template.
            private static RouteValueDictionary GetDefaults(RouteTemplate parsedTemplate)
            {
                var result = new RouteValueDictionary();

                foreach (var parameter in parsedTemplate.Parameters)
                {
                    if (parameter.DefaultValue != null)
                    {
                        result.Add(parameter.Name, parameter.DefaultValue);
                    }
                }

                return result;
            }

            public static string GetKeyFromRequest(string routeTemplate, string key, Uri request)
            {
                return GetKeyFromRequest(routeTemplate, key, request, s => s);
            }

            public static Guid GetGuidKeyFromRequest(string routeTemplate, string key, Uri request)
            {
                return GetKeyFromRequest(routeTemplate, key, request, Guid.Parse);
            }

            public static T GetKeyFromRequest<T>(string routeTemplate, string key, Uri request, Func<string, T> keyFromString)
            {
                RouteValueDictionary dict;
                if (!TryMatch(routeTemplate, request, out dict))
                {
                    throw new ArgumentException($"Unexpected uri '{request}'. Expected uri for template: {routeTemplate}");
                }

                object value;
                if (!dict.TryGetValue(key, out value))
                {
                    throw new ArgumentException($"Key {key} not found in {request}");
                }

                return keyFromString((string)value);
            }
        }
}