using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Routing.Template;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebApi.HypermediaExtensions.Test.JsonSchema
{
    public class JsonDeserializer
    {
        readonly Type type;
        readonly ImmutableDictionary<Type, TemplateMatcher> templateMatchers;
        ImmutableArray<KeyFromUriProperty> keyFromUriProperties;

        public JsonDeserializer(Type type, Func<Type, string> getRouteTemplateForType)
        {
            this.type = type;
            
            keyFromUriProperties = type.GetKeyFromUriProperties();

            templateMatchers = keyFromUriProperties.Select(p => p.TargetType).Distinct()
                .ToImmutableDictionary(t => t, t => RouteMatcher.GetTemplateMatcher(getRouteTemplateForType(t)));

            //TODO: validate that all template paraemter names use in properties are present as route template parameters in corresponding template
            //var keyPropertiesMissingInTemplate = keyFromUriProperties
            //    .Select(k => k.ResolvedRouteTemplateParameterName)
            //    .Except(templateMatcher.Template.Parameters.Select(p => p.Name)).ToImmutableArray();
            //if (keyPropertiesMissingInTemplate.Any())
            //{
            //    throw new ArgumentException($"Type {type.BeautifulName()} containes KeyFromUri properties that are not represented as route template parameters: {string.Join(",", keyPropertiesMissingInTemplate)}");
            //}
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
            foreach (var propertyByUriProperty in keyFromUriProperties.GroupBy(p => new {p.SchemaPropertyName, p.TargetType}))
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
                    throw new ArgumentException(
                        $"Lacal path '{request.LocalPath}' does not match expected route template '{templateMatcher.Template.TemplateText}'");
                }

                raw.Remove(uriPropertyName);
                foreach (var keyFromUriProperty in propertyByUriProperty)
                {
                    raw.Add(new JProperty(keyFromUriProperty.PropertyInfo.Name,
                        (string)values[keyFromUriProperty.ResolvedRouteTemplateParameterName]));
                }
            }

            return raw.ToObject(type);
        }
    }
}