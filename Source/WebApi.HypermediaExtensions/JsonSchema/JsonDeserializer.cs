using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApi.HypermediaExtensions.Exceptions;
using WebApi.HypermediaExtensions.Util;
using WebApi.HypermediaExtensions.WebApi.RouteResolver;

namespace WebApi.HypermediaExtensions.JsonSchema
{
    public class JsonDeserializer
    {
        readonly Type type;
        readonly ImmutableArray<KeyPropertiesOfSchema> keyFromUriProperties;

        public JsonDeserializer(Type type, Func<Type, string> getRouteTemplateForType) : this(type, t => ImmutableArray.Create(getRouteTemplateForType(t)))
        {
        }

        public JsonDeserializer(Type type, Func<Type, ImmutableArray<string>> getRouteTemplateForType)
        {
            this.type = type;

            keyFromUriProperties = type
                .GetKeyFromUriProperties()
                .GroupBy(k => k.SchemaPropertyName)
                .Select(_ => new KeyPropertiesOfSchema(_.Key, _, getRouteTemplateForType))
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
            if (raw == null)
            {
                return null;
            }

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

                RouteValueDictionary values = null;
                if (!schemaProperyGroup.TemplateMatchers.Any(t => t.TryGetValuesFromRequest(request.LocalPath, out values)))
                {
                    //trim first path part if application is hosted with a base path part (only one supported...). Passing the base path from configuration would be the better approach.
                    var basePathTrimmed = TrimFirstPathPart(request.LocalPath);
                    if (!schemaProperyGroup.TemplateMatchers.Any(t => t.TryGetValuesFromRequest(basePathTrimmed, out values)))
                    {
                        if (request.LocalPath.Contains("[Area]") || request.LocalPath.Contains("[area]"))
                        {
                            throw new ArgumentException($"Local path '{request.LocalPath}' contains unsupported tokens. The tokens '[Area]' and '[area]' are not supported. Please replace them with fixed values.");
                        }

                        throw new ArgumentException($"Local path '{request.LocalPath}' does not match any expected route template '{string.Join(",", schemaProperyGroup.TemplateMatchers.Select(r => r.Template.TemplateText))}'");
                    }
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

        static string TrimFirstPathPart(string requestLocalPath)
        {
            return requestLocalPath.Substring(requestLocalPath.IndexOf('/', 1));
        }

        class KeyPropertiesOfSchema
        {
            public string SchemaPropertyName { get; }
            public Type TargetType { get; }
            public bool IsRequired { get; }
            public ImmutableArray<TemplateMatcher> TemplateMatchers { get; }
            public ImmutableArray<KeyFromUriProperty> Properties { get; }

            public KeyPropertiesOfSchema(string schemaPropertyName, IEnumerable<KeyFromUriProperty> properties, Func<Type, ImmutableArray<string>> getTemplates)
            {
                SchemaPropertyName = schemaPropertyName;
                Properties = properties.ToImmutableArray();
                TargetType = Properties.Select(p => p.TargetType).Distinct().Single();
                IsRequired = Properties.Any(p => p.Property.GetCustomAttribute<RequiredAttribute>() != null);

                var templates = getTemplates(TargetType);
                if (templates == null || templates.Length == 0)
                {
                    throw new HypermediaException($"No Get route found for hypermedia type '{TargetType.BeautifulName()}'");
                }

                TemplateMatchers = templates.Select(RouteMatcher.GetTemplateMatcher).ToImmutableArray();

                if (Properties.Length == 1)
                {
                    var property = Properties[0];
                    if (property.RouteTemplateParameterName == null &&
                        TemplateMatchers.All(t => t.Template.Parameters.Count == 1))
                    {
                        var templateParameterNames = TemplateMatchers.Select(t => t.Template.Parameters.Select(p => p.Name).First()).Distinct().ToImmutableArray();
                        if (templateParameterNames.Length > 1)
                        {
                            throw new HypermediaException($"Different template parameter names used in routes to objects with same base type '{TargetType.BeautifulName()}': {string.Join(",", templateParameterNames)}");
                        }

                        Properties = ImmutableArray.Create(new KeyFromUriProperty(property.TargetType, property.Property, property.SchemaPropertyName, templateParameterNames[0]));
                    }
                }

                var keyPropertiesMissingInTemplate = Properties
                    .Where(k => TemplateMatchers.Any(t => t.Template.Parameters.All(p => p.Name != k.ResolvedRouteTemplateParameterName)))
                    .ToImmutableArray();
                if (keyPropertiesMissingInTemplate.Any())
                {
                    throw new ArgumentException($"Type {Properties.First().Property.DeclaringType.BeautifulName()} contains KeyFromUri properties that are not represented as route template parameters: {string.Join(",", keyPropertiesMissingInTemplate.Select(k => $"Property {k.Property.Name}"))}. Route templates: {string.Join(",", TemplateMatchers.Select(r => r.Template.TemplateText))}");
                }
            }
        }
    }
}