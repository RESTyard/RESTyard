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
using RESTyard.AspNetCore.Exceptions;
using RESTyard.AspNetCore.Util;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace RESTyard.AspNetCore.JsonSchema
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
                .Select(_ => new KeyPropertiesOfSchema(_.Key, _, getRouteTemplateForType, _.First().NestingPropertyNames))
                .ToImmutableArray();
        }

        public object? Deserialize(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                var raw = (JObject?)new JsonSerializer().Deserialize(jsonTextReader);
                return Deserialize(raw);
            }
        }

        public object? Deserialize(JObject? raw)
        {
            if (raw == null)
            {
                return null;
            }
            
            foreach (var schemaPropertyGroup in keyFromUriProperties)
            {
                var allAreCollection = schemaPropertyGroup.Properties
                    .All(p => p.Property.PropertyType.GetInterfaces()
                                  .Any(x => x.IsGenericType &&
                                            x.GetGenericTypeDefinition() == typeof(ICollection<>))
                              && p.TargetType == schemaPropertyGroup.Properties.First().TargetType);
                var allAreNotCollection = schemaPropertyGroup.Properties
                    .All(p => !p.Property.PropertyType.GetInterfaces()
                                  .Any(x => x.IsGenericType &&
                                            x.GetGenericTypeDefinition() == typeof(ICollection<>))
                              && p.TargetType == schemaPropertyGroup.Properties.First().TargetType);
                bool? isSingleUriDeconstruction = allAreCollection ? false : allAreNotCollection ? true : null;
                if (isSingleUriDeconstruction is null)
                {
                    throw new Exception("Attribute KeyFromUri should be applied consistently either as a List or not as a List for a schema name.");
                }

                var uriPropertyName = schemaPropertyGroup.SchemaPropertyName;
                if (!raw.TryGetNestedValue(uriPropertyName, schemaPropertyGroup.NestingPropertyNameSpace, out var uriToken))
                {
                    if (!schemaPropertyGroup.IsRequired)
                        continue;
                    throw new ArgumentException($"Required uri property {uriPropertyName} is missing");
                }

                var uris = isSingleUriDeconstruction is true ? [uriToken.ToString()] : uriToken.ToObject<List<string>>();
                var parameterValues = new Dictionary<string, List<string?>>();
                foreach (var uri in uris)
                {
                    if (!Uri.TryCreate(uri, UriKind.RelativeOrAbsolute, out var request))
                    {
                        throw new ArgumentException($"Value property {uriPropertyName} is not a valid uri. Value: '{uri}'");
                    }

                    RouteValueDictionary? values = null;
                    if (!schemaPropertyGroup.TemplateMatchers.Any(t => t.TryGetValuesFromRequest(request.LocalPath, out values)))
                    {
                        //trim first path part if application is hosted with a base path part (only one supported...). Passing the base path from configuration would be the better approach.
                        var basePathTrimmed = TrimFirstPathPart(request.LocalPath);
                        if (!schemaPropertyGroup.TemplateMatchers.Any(t => t.TryGetValuesFromRequest(basePathTrimmed, out values)))
                        {
                            if (request.LocalPath.Contains("[Area]") || request.LocalPath.Contains("[area]"))
                            {
                                throw new ArgumentException($"Local path '{request.LocalPath}' contains unsupported tokens. The tokens '[Area]' and '[area]' are not supported. Please replace them with fixed values.");
                            }

                            throw new ArgumentException($"Local path '{request.LocalPath}' does not match any expected route template '{string.Join(",", schemaPropertyGroup.TemplateMatchers.Select(r => r.Template.TemplateText))}'");
                        }
                    }

                    raw.RemoveNested(uriPropertyName, schemaPropertyGroup.NestingPropertyNameSpace);
                    foreach (var keyFromUriProperty in schemaPropertyGroup.Properties)
                    {
                        var parameterValue = (string?)values![keyFromUriProperty.ResolvedRouteTemplateParameterName];
                        if (!parameterValues.ContainsKey(keyFromUriProperty.ResolvedRouteTemplateParameterName))
                        {
                            parameterValues[keyFromUriProperty.ResolvedRouteTemplateParameterName] = [];
                        }
                        parameterValues[keyFromUriProperty.ResolvedRouteTemplateParameterName].Add(parameterValue);
                    }
                }

                foreach (var keyFromUriProperty in schemaPropertyGroup.Properties)
                {
                    var parameterValue = parameterValues[keyFromUriProperty.ResolvedRouteTemplateParameterName];
                    raw.SetNestedValue(keyFromUriProperty.Property.Name, keyFromUriProperty.NestingPropertyNames, parameterValue, isSingleUriDeconstruction!.Value);
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
            public ImmutableArray<string> NestingPropertyNameSpace { get; }
            public Type TargetType { get; }
            public bool IsRequired { get; }
            public ImmutableArray<TemplateMatcher> TemplateMatchers { get; }
            public ImmutableArray<KeyFromUriProperty> Properties { get; }

            public KeyPropertiesOfSchema(
                string schemaPropertyName,
                IEnumerable<KeyFromUriProperty> properties,
                Func<Type, ImmutableArray<string>> getTemplates,
                ImmutableArray<string> nestingPropertyNameSpace)
            {
                SchemaPropertyName = schemaPropertyName;
                NestingPropertyNameSpace = nestingPropertyNameSpace;
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

                        Properties = ImmutableArray.Create(new KeyFromUriProperty(property.TargetType, property.Property, property.SchemaPropertyName, templateParameterNames[0], NestingPropertyNameSpace));
                    }
                }

                var keyPropertiesMissingInTemplate = Properties
                    .Where(k => TemplateMatchers.Any(t => t.Template.Parameters.All(p => p.Name != k.ResolvedRouteTemplateParameterName)))
                    .ToImmutableArray();
                if (keyPropertiesMissingInTemplate.Any())
                {
                    throw new ArgumentException($"Type {Properties.First().Property.DeclaringType?.BeautifulName()} contains KeyFromUri properties that are not represented as route template parameters: {string.Join(",", keyPropertiesMissingInTemplate.Select(k => $"Property {k.Property.Name}"))}. Route templates: {string.Join(",", TemplateMatchers.Select(r => r.Template.TemplateText))}");
                }
            }
        }
    }
}