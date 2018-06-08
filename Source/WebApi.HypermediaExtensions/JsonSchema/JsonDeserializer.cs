using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Routing.Template;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApi.HypermediaExtensions.Util;

namespace WebApi.HypermediaExtensions.JsonSchema
{
    public class JsonDeserializer
    {
        readonly Type type;
        readonly ImmutableArray<KeyPropertiesOfSchemaProperty> keyFromUriProperties;

        public JsonDeserializer(Type type, Func<Type, string> getRouteTemplateForType)
        {
            this.type = type;

            keyFromUriProperties = type
                .GetKeyFromUriProperties()
                .GroupBy(k => k.SchemaPropertyName)
                .Select(_ => new KeyPropertiesOfSchemaProperty(_.Key, _, getRouteTemplateForType))
                .ToImmutableArray();
        }

        public object Deserialize(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                var raw = (JObject)new JsonSerializer().Deserialize(jsonTextReader);
                return Deserialize(raw);
            }
        }

        public object Deserialize(JObject raw)
        {
            foreach (var schemaProperyGroup in keyFromUriProperties)
            {
                var uriPropertyName = schemaProperyGroup.SchemaPropertyName;
                if (!raw.TryGetValue(uriPropertyName, out var uriToken))
                {
                    if (!schemaProperyGroup.IsRequired)
                        continue;
                    throw new ArgumentException($"Required uri property {uriPropertyName} is missing");
                }

                var uri = (string)uriToken;
                if (!Uri.TryCreate(uri, UriKind.RelativeOrAbsolute, out var request))
                {
                    throw new ArgumentException($"Value property {uriPropertyName} is not a valid uri. Value: '{uri}'");
                }

                if (!schemaProperyGroup.TemplateMatcher.TryGetValuesFromRequest(request.LocalPath, out var values))
                {
                    throw new ArgumentException($"Local path '{request.LocalPath}' does not match expected route template '{schemaProperyGroup.TemplateMatcher.Template.TemplateText}'");
                }

                raw.Remove(uriPropertyName);
                foreach (var keyFromUriProperty in schemaProperyGroup.Properties)
                {
                    var parameterValue = (string)values[keyFromUriProperty.ResolvedRouteTemplateParameterName];
                    raw.Add(new JProperty(keyFromUriProperty.Property.Name,
                        parameterValue));
                }
            }

            return raw.ToObject(type);
        }

        class KeyPropertiesOfSchemaProperty
        {
            public string SchemaPropertyName { get; }
            public Type TargetType { get; }
            public bool IsRequired { get; }
            public TemplateMatcher TemplateMatcher { get; }
            public ImmutableArray<KeyFromUriProperty> Properties { get; }

            public KeyPropertiesOfSchemaProperty(string schemaPropertyName, IEnumerable<KeyFromUriProperty> properties, Func<Type, string> getTemplate)
            {
                SchemaPropertyName = schemaPropertyName;
                Properties = properties.ToImmutableArray();
                TargetType = Properties.Select(p => p.TargetType).Distinct().Single();
                IsRequired = Properties.Any(p => p.Property.GetCustomAttribute<RequiredAttribute>() != null);
                TemplateMatcher = RouteMatcher.GetTemplateMatcher(getTemplate(TargetType));

                if (Properties.Length == 1)
                {
                    var property = Properties[0];
                    if (property.RouteTemplateParameterName == null && TemplateMatcher.Template.Parameters.Count == 1)
                        Properties = ImmutableArray.Create(new KeyFromUriProperty(property.TargetType, property.Property, property.SchemaPropertyName, TemplateMatcher.Template.Parameters.First().Name));
                }

                var keyPropertiesMissingInTemplate = Properties
                    .Where(k => TemplateMatcher.Template.Parameters.All(p => p.Name != k.ResolvedRouteTemplateParameterName))
                    .ToImmutableArray();
                if (keyPropertiesMissingInTemplate.Any())
                {
                    throw new ArgumentException($"Type {Properties.First().Property.DeclaringType.BeautifulName()} contains KeyFromUri properties that are not represented as route template parameters: {string.Join(",", keyPropertiesMissingInTemplate.Select(k => $"Property {k.Property.Name}"))}");
                }
            }
        }
    }
}