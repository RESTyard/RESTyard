using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Routing.Template;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApi.HypermediaExtensions.Util;

namespace WebApi.HypermediaExtensions.JsonSchema
{
    public class JsonDeserializer
    {
        readonly Type type;
        readonly ImmutableDictionary<Type, TemplateMatcher> templateMatchers;
        ImmutableArray<KeyFromUriProperty> keyFromUriProperties;

        public JsonDeserializer(Type type, Func<Type, string> getRouteTemplateForType)
        {
            this.type = type;

            keyFromUriProperties = type
                .GetKeyFromUriProperties();

            templateMatchers = keyFromUriProperties.Select(p => p.TargetType).Distinct()
                .ToImmutableDictionary(t => t, t => RouteMatcher.GetTemplateMatcher(getRouteTemplateForType(t)));

            keyFromUriProperties = ResolveParameterNamesForSingleKeyProperties();

            var keyPropertiesMissingInTemplate = keyFromUriProperties
                .Where(k => templateMatchers[k.TargetType].Template.Parameters.All(p => p.Name != k.ResolvedRouteTemplateParameterName))
                .ToImmutableArray();
            if (keyPropertiesMissingInTemplate.Any())
            {
                throw new ArgumentException($"Type {type.BeautifulName()} contains KeyFromUri properties that are not represented as route template parameters: {string.Join(",", keyPropertiesMissingInTemplate.Select(k => $"Property {k.Property.Name}"))}");
            }
        }

        ImmutableArray<KeyFromUriProperty> ResolveParameterNamesForSingleKeyProperties()
        {
            return keyFromUriProperties.GroupBy(p => p.SchemaPropertyName)
                .SelectMany(g =>
                {
                    if (g.Count() == 1)
                    {
                        return g.Select(k =>
                        {
                            if (k.RouteTemplateParameterName == null && templateMatchers[k.TargetType].Template.Parameters.Count == 1)
                                return new KeyFromUriProperty(k.TargetType, k.Property, k.SchemaPropertyName, templateMatchers[k.TargetType].Template.Parameters.First().Name);
                            return k;
                        });
                    }

                    return g;
                }).ToImmutableArray();
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
            foreach (var propertyByUriProperty in keyFromUriProperties.GroupBy(p => new { p.SchemaPropertyName, p.TargetType }))
            {
                var uriPropertyName = propertyByUriProperty.Key.SchemaPropertyName;
                if (!raw.TryGetValue(uriPropertyName, out var uriToken))
                {
                    throw new ArgumentException($"Required uri property {uriPropertyName} is missing");
                }

                var templateMatcher = templateMatchers[propertyByUriProperty.Key.TargetType];
                var uri = (string)uriToken;

                if (!Uri.TryCreate(uri, UriKind.RelativeOrAbsolute, out var request))
                {
                    throw new ArgumentException($"Value property {uriPropertyName} is not a valid uri. Value: '{uri}'");
                }

                if (!templateMatcher.TryGetValuesFromRequest(request.LocalPath, out var values))
                {
                    throw new ArgumentException($"Local path '{request.LocalPath}' does not match expected route template '{templateMatcher.Template.TemplateText}'");
                }

                raw.Remove(uriPropertyName);
                foreach (var keyFromUriProperty in propertyByUriProperty)
                {
                    var parameterValue = (string)values[keyFromUriProperty.ResolvedRouteTemplateParameterName];
                    raw.Add(new JProperty(keyFromUriProperty.Property.Name,
                        parameterValue));
                }
            }

            return raw.ToObject(type);
        }
    }
}