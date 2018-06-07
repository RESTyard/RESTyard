using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Routing.Template;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApi.HypermediaExtensions.Util;

namespace WebApi.HypermediaExtensions.Test.JsonSchema
{
    public class JsonDeserializer
    {
        readonly Type type;
        readonly TemplateMatcher templateMatcher;
        ImmutableArray<KeyFromUriProperty> keyFromUriProperties;

        public JsonDeserializer(Type type, string routeTemplate)
        {
            this.type = type;
            templateMatcher = RouteMatcher.GetTemplateMatcher(routeTemplate);
            keyFromUriProperties = type.GetKeyFromUriProperties();

            var keyPropertiesMissingInTemplate = keyFromUriProperties
                .Select(k => k.ResolvedRouteTemplateParameterName)
                .Except(templateMatcher.Template.Parameters.Select(p => p.Name)).ToImmutableArray();
            if (keyPropertiesMissingInTemplate.Any())
            {
                throw new ArgumentException($"Type {type.BeautifulName()} containes KeyFromUri properties that are not represented as route template parameters: {string.Join(",", keyPropertiesMissingInTemplate)}");
            }
        }

        public object Deserialize(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                var raw = (JObject)new JsonSerializer().Deserialize(jsonTextReader);
                foreach (var propertyByUriProperty in keyFromUriProperties.GroupBy(p => p.SchemaPropertyName))
                {
                    var uriPropertyName = propertyByUriProperty.Key;
                    if (!raw.TryGetValue(uriPropertyName, out var uriToken))
                    {
                        throw new ArgumentException($"Required uri property {uriPropertyName} is missing");
                    }

                    var uri = (string)uriToken;

                    if (!Uri.TryCreate(uri, UriKind.RelativeOrAbsolute, out var request))
                    {
                        throw new ArgumentException($"Value property {uriPropertyName} is not a valid uri. Value: '{uri}'");
                    }

                    if (!templateMatcher.TryGetValuesFromRequest(request.LocalPath, out var values))
                    {
                        throw new ArgumentException($"Lacal path '{request.LocalPath}' does not match expected route template '{templateMatcher.Template.TemplateText}'");
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
}