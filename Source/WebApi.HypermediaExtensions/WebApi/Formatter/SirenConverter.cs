using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Hypermedia.MediaTypes;
using Newtonsoft.Json.Linq;
using WebApi.HypermediaExtensions.Exceptions;
using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.Hypermedia.Actions;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;
using WebApi.HypermediaExtensions.Hypermedia.Links;
using WebApi.HypermediaExtensions.Query;
using WebApi.HypermediaExtensions.Util;
using WebApi.HypermediaExtensions.Util.Enum;
using WebApi.HypermediaExtensions.WebApi.RouteResolver;

namespace WebApi.HypermediaExtensions.WebApi.Formatter
{
    public class SirenConverter : IHypermediaJsonConverter, IHypermediaConverter
    {
        private static readonly ISirenConverterConfiguration defaultConfiguration = new SirenConverterConfiguration();

        private readonly IQueryStringBuilder queryStringBuilder;
        readonly ApplicationModel applicationModel;
        private readonly IHypermediaRouteResolver routeResolver;
        private readonly ISirenConverterConfiguration configuration;

        public SirenConverter(IHypermediaRouteResolver routeResolver, IQueryStringBuilder queryStringBuilder,
            ApplicationModel applicationModel, ISirenConverterConfiguration configuration = null)
        {
            this.queryStringBuilder = queryStringBuilder;
            this.applicationModel = applicationModel;
            this.routeResolver = routeResolver;

            this.configuration = configuration ?? defaultConfiguration;
        }

        public string ConvertToString(HypermediaObject hypermediaObject)
        {
            return ConvertToJson(hypermediaObject).ToString();
        }

        public JObject ConvertToJson(HypermediaObject hypermediaObject)
        {
            return CreateSirenInternal(hypermediaObject);
        }

        private JObject CreateSirenInternal(HypermediaObject hypermediaObject, bool isEmbedded = false, List<string> embeddedEntityRelations = null)
        {
            var sirenJson = new JObject();

            if (!applicationModel.HmoTypes.TryGetValue(hypermediaObject.GetType(), out var hmoType))
            {
                throw new HypermediaException($"Type {hypermediaObject.GetType()} not registered in ApplicationModel");
            }

            AddClasses(hypermediaObject, sirenJson, hmoType.Classes);
            AddTitle(sirenJson, hmoType.Title);

            if (isEmbedded)
            {
                if (embeddedEntityRelations == null)
                {
                    throw new HypermediaException("Embedded Entity has no relations.");
                }

                AddEmbeddedEntityRelations(sirenJson, embeddedEntityRelations);
            }

            AddProperties(hypermediaObject, hmoType.Properties, sirenJson);
            SirenAddEntities(hypermediaObject, sirenJson);
            AddActions(hypermediaObject, sirenJson);
            AddLinks(hypermediaObject, sirenJson);

            return sirenJson;
        }

        private static HypermediaObjectAttribute GetHypermediaObjectAttribute(HypermediaObject hypermediaObject)
        {
            return GetHypermediaObjectAttribute(hypermediaObject.GetType());
        }

        private static HypermediaObjectAttribute GetHypermediaObjectAttribute(Type hypermediaObjectType)
        {
            return hypermediaObjectType.GetTypeInfo().GetCustomAttribute<HypermediaObjectAttribute>();
        }

        private static HypermediaPropertyAttribute GetHypermediaPropertyAttribute(PropertyInfo hypermediaPropertyInfo)
        {
            return hypermediaPropertyInfo.GetCustomAttribute<HypermediaPropertyAttribute>();
        }

        private void AddActions(HypermediaObject hypermediaObject, JObject sirenJson)
        {
            var properties = hypermediaObject.GetType().GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var jActions = new JArray();

            foreach (var property in properties)
            {
                if (PropertyHasIgnoreAttribute(property))
                {
                    continue;
                }
                if (!IsHypermediaAction(property))
                {
                    continue;
                }

                var action = property.GetValue(hypermediaObject);
                var actionBase = (HypermediaActionBase)action;

                if (actionBase.CanExecute())
                {
                    var jAction = new JObject();
                    AddGeneralSirenActionProperties(jAction, property, hypermediaObject, actionBase);

                    AddActionParameters(actionBase, jAction);

                    jActions.Add(jAction);
                }
            }

            sirenJson.Add("actions", jActions);
        }

        private void AddGeneralSirenActionProperties(JObject jAction, PropertyInfo property, HypermediaObject hypermediaObject, HypermediaActionBase actionBase)
        {
            var hypermediaActionAttribute = GetHypermediaActionAttribute(property);

            string actionName;
            if (!string.IsNullOrEmpty(hypermediaActionAttribute?.Name))
            {
                actionName = hypermediaActionAttribute.Name;
            }
            else
            {
                actionName = property.Name;
            }
            jAction.Add("name", actionName);

            if (!string.IsNullOrEmpty(hypermediaActionAttribute?.Title))
            {
                jAction.Add("title", hypermediaActionAttribute.Title);
            }

            jAction.Add("method", "POST");
            jAction.Add("href", routeResolver.ActionToRoute(hypermediaObject, actionBase));
        }

        private static HypermediaActionAttribute GetHypermediaActionAttribute(PropertyInfo hypermediaActionPropertyInfo)
        {
            return hypermediaActionPropertyInfo.GetCustomAttribute<HypermediaActionAttribute>();
        }

        private void AddActionParameters(HypermediaActionBase hypermediaAction, JObject jAction)
        {
            if (!hypermediaAction.HasParameter())
            {
                return;
            }

            jAction.Add("type", DefaultMediaTypes.ApplicationJson);
            AddActionFields(jAction, hypermediaAction.ParameterType());
        }

        private void AddActionFields(JObject jAction, Type actionParameterType)
        {
            var jfield = new JObject
            {
                {"name", actionParameterType.BeautifulName() },
                {"type", DefaultMediaTypes.ApplicationJson}
            };

            if (!routeResolver.TryGetRouteByType(actionParameterType, out var classRoute))
            {
                classRoute = routeResolver.RouteUrl(RouteNames.ActionParameterTypes, new { parameterTypeName = actionParameterType.BeautifulName() });
            }

            jfield.Add("class", new JArray { classRoute });

            jAction.Add("fields", new JArray { jfield });
        }

        private static bool IsHypermediaAction(PropertyInfo property)
        {
            return typeof(HypermediaActionBase).GetTypeInfo().IsAssignableFrom(property.PropertyType);
        }

        private void SirenAddEntities(HypermediaObject hypermediaObject, JObject sirenJson)
        {
            var embeddedEntities = hypermediaObject.Entities;
            var jEntities = new JArray();

            foreach (var embeddedEntity in embeddedEntities)
            {
                if (embeddedEntity.Reference.IsResolved())
                {
                    var entitySiren = CreateSirenInternal(embeddedEntity.Reference.GetInstance(), true, embeddedEntity.Relations);
                    jEntities.Add(entitySiren);
                }
                else
                {
                    var jLink = new JObject();

                    var entityType = embeddedEntity.Reference.GetHypermediaType();
                    var hypermediaObjectAttribute = GetHypermediaObjectAttribute(entityType);
                    AddClasses(entityType, jLink, hypermediaObjectAttribute);
                    AddEmbeddedEntityRelations(jLink, embeddedEntity.Relations);

                    var resolvedAdress = ResolveReferenceRoute(embeddedEntity.Reference);
                    jLink.Add("href", resolvedAdress);
                    jEntities.Add(jLink);
                }
            }

            sirenJson.Add("entities", jEntities);
        }

        private void AddLinks(HypermediaObject hypermediaObject, JObject sirenJson)
        {
            var hypermediaLinks = hypermediaObject.Links;
            var jLinks = new JArray();

            foreach (var hypermediaLink in hypermediaLinks)
            {
                var jLink = new JObject();

                var jRel = new JArray { hypermediaLink.Key };
                jLink.Add("rel", jRel);

                var resolvedAdress = ResolveReferenceRoute(hypermediaLink.Value.Reference);

                jLink.Add("href", resolvedAdress);

                jLinks.Add(jLink);
            }

            sirenJson.Add("links", jLinks);
        }

        private string ResolveReferenceRoute(HypermediaObjectReferenceBase reference)
        {
            var resolvedAdress = routeResolver.ReferenceToRoute(reference);
            var query = reference.GetQuery();
            resolvedAdress += queryStringBuilder.CreateQueryString(query);
            return resolvedAdress;
        }

        private void AddProperties(HypermediaObject hypermediaObject, ImmutableArray<ApplicationModel.HmoProperty> properties, JObject sirenJson)
        {
            var jProperties = new JObject();
            foreach (var property in properties)
            {
                AddProperty(hypermediaObject, property, jProperties);
            }

            sirenJson.Add("properties", jProperties);
        }

        private void AddProperty(HypermediaObject hypermediaObject, ApplicationModel.HmoProperty property, JObject jProperties)
        {
            var propertyPropertyInfo = property.PropertyInfo;
            var propertyType = property.PropertyInfo.PropertyType;
            var propertyTypeInfo = propertyType.GetTypeInfo();
            var value = propertyPropertyInfo.GetValue(hypermediaObject);
            var propertyName = property.Name;

            if (value == null)
            {
                if (configuration.WriteNullProperties)
                {
                    jProperties.Add(propertyName, null);
                }
                return;
            }

            if (propertyTypeInfo.IsValueType)
            {
                if (propertyTypeInfo.IsEnum)
                {
                    var enumAsString = EnumHelper.GetEnumMemberValue(propertyType, value);
                    jProperties.Add(propertyName, new JValue(enumAsString));
                }
                else
                {
                    jProperties.Add(propertyName, new JValue(value));
                }

                return;
            }

            if (IsTimeType(propertyType))
            {
                jProperties.Add(propertyName, ((IFormattable)value).ToString("o", CultureInfo.InvariantCulture));
                return;
            }

            jProperties.Add(propertyName, string.Format(CultureInfo.InvariantCulture, "{0}", value));
        }

        private static string GetPropertyName(PropertyInfo publicProperty)
        {
            string propertyName;
            var hypermediaPropertyAttribute = GetHypermediaPropertyAttribute(publicProperty);
            if (!string.IsNullOrEmpty(hypermediaPropertyAttribute?.Name))
            {
                propertyName = hypermediaPropertyAttribute.Name;
            }
            else
            {
                propertyName = publicProperty.Name;
            }
            return propertyName;
        }

        private static bool IsTimeType(Type type)
        {
            return type == typeof(DateTime) || type == typeof(DateTimeOffset);
        }

        private static bool PropertyHasIgnoreAttribute(PropertyInfo publicProperty)
        {
            return publicProperty.CustomAttributes.Any(a => a.AttributeType == typeof(FormatterIgnoreHypermediaPropertyAttribute));
        }

        private static void AddClasses(HypermediaObject hypermediaObject, JObject sirenJson, ImmutableArray<string> classes)
        {
            AddClasses(hypermediaObject.GetType(), sirenJson, classes);
        }

        private static void AddClasses(Type hypermediaObjectType, JObject sirenJson, ImmutableArray<string> classes)
        {
            var sirenClasses = new JArray();

            if (classes.Length > 0)
            {
                foreach (var hypermediaClass in classes)
                {
                    sirenClasses.Add(hypermediaClass);
                }
            }
            else
            {
                var hypermediaObjectName = hypermediaObjectType.Name;
                sirenClasses.Add(hypermediaObjectName);
            }

            sirenJson.Add("class", sirenClasses);
        }

        private static void AddEmbeddedEntityRelations(JObject jembeddedEntity, List<string> embeddedEntityRelations)
        {
            var rels = new JArray();
            foreach (var embeddedEntityRelation in embeddedEntityRelations)
            {
                rels.Add(embeddedEntityRelation);
            }
            jembeddedEntity.Add("rel", rels);
        }

        private static void AddTitle(JObject sirenJson, string title)
        {
            if (!string.IsNullOrEmpty(title))
            {
                sirenJson.Add("title", title);
            }
        }
    }
}