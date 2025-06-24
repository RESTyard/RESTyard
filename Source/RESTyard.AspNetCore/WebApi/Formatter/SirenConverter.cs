using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using RESTyard.AspNetCore.Exceptions;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.AspNetCore.Hypermedia.Attributes;
using RESTyard.AspNetCore.Hypermedia.Links;
using RESTyard.AspNetCore.Query;
using RESTyard.AspNetCore.Util;
using RESTyard.AspNetCore.Util.Enum;
using RESTyard.AspNetCore.WebApi.AttributedRoutes;
using RESTyard.AspNetCore.WebApi.ExtensionMethods;
using RESTyard.AspNetCore.WebApi.RouteResolver;
using RESTyard.MediaTypes;

namespace RESTyard.AspNetCore.WebApi.Formatter
{
    public class SirenConverter : IHypermediaJsonConverter, IHypermediaConverter
    {
        private static readonly HypermediaConverterConfiguration DefaultConfiguration =
            new HypermediaConverterConfiguration();

        private readonly IQueryStringBuilder queryStringBuilder;
        private readonly IHypermediaRouteResolver routeResolver;
        private readonly HypermediaConverterConfiguration configuration;
        private static readonly Type HypermediaActionBaseType = typeof(HypermediaActionBase).GetTypeInfo();

        public SirenConverter(
            IHypermediaRouteResolver routeResolver,
            IQueryStringBuilder queryStringBuilder,
            HypermediaConverterConfiguration? configuration = null)
        {
            this.queryStringBuilder = queryStringBuilder;
            this.routeResolver = routeResolver;

            this.configuration = configuration ?? DefaultConfiguration;
        }

        public string ConvertToString(IHypermediaObject hypermediaObject)
        {
            return ConvertToJson(hypermediaObject).ToString();
        }

        public JObject ConvertToJson(IHypermediaObject hypermediaObject)
        {
            return CreateSirenInternal(hypermediaObject);
        }

        private JObject CreateSirenInternal(
            IHypermediaObject hypermediaObject,
            bool isEmbedded = false,
            IReadOnlyCollection<string>? embeddedEntityRelations = null)
        {
            var sirenJson = new JObject();

            var hypermediaObjectAttribute = GetHypermediaObjectAttribute(hypermediaObject);
            AddClasses(hypermediaObject, sirenJson, hypermediaObjectAttribute);
            AddTitle(sirenJson, hypermediaObjectAttribute);

            if (isEmbedded)
            {
                if (embeddedEntityRelations == null)
                {
                    throw new HypermediaException("Embedded Entity has no relations.");
                }

                AddEmbeddedEntityRelations(sirenJson, embeddedEntityRelations);
            }

            AddProperties(hypermediaObject, sirenJson);
            SirenAddEntities(hypermediaObject, sirenJson);
            AddActions(hypermediaObject, sirenJson);
            AddLinks(hypermediaObject, sirenJson);
            AssertNoBaseRelatedEntities(hypermediaObject);

            return sirenJson;
        }

        private static HypermediaObjectAttribute? GetHypermediaObjectAttribute(IHypermediaObject hypermediaObject)
        {
            return GetHypermediaObjectAttribute(hypermediaObject.GetType());
        }

        private static HypermediaObjectAttribute? GetHypermediaObjectAttribute(Type hypermediaObjectType)
        {
            return hypermediaObjectType.GetTypeInfo().GetCustomAttribute<HypermediaObjectAttribute>();
        }

        private static HypermediaPropertyAttribute? GetHypermediaPropertyAttribute(PropertyInfo hypermediaPropertyInfo)
        {
            return hypermediaPropertyInfo.GetCustomAttribute<HypermediaPropertyAttribute>();
        }

        private void AddActions(IHypermediaObject hypermediaObject, JObject sirenJson)
        {
            var properties = hypermediaObject
                .GetType()
                .GetTypeInfo()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => !PropertyHasIgnoreAttribute(property))
                .Where(IsHypermediaAction);

            var jActions = new JArray();

            foreach (var property in properties)
            {
                var action = property.GetValue(hypermediaObject);

                if (action is HypermediaActionBase actionBase && actionBase.CanExecute())
                {
                    AddActionSirenContent(hypermediaObject, actionBase, property, jActions);
                }
            }

            sirenJson.Add("actions", jActions);
        }

        private void AddActionSirenContent(
            IHypermediaObject hypermediaObject,
            HypermediaActionBase actionBase,
            PropertyInfo property,
            JArray jActions)
        {
            var jAction = new JObject();
            ResolvedRoute resolvedRoute;
            if (actionBase is HypermediaExternalActionBase externalActionBase)
            {
                resolvedRoute = new ResolvedRoute(
                    externalActionBase.ExternalUri.ToString(),
                    externalActionBase.HttpMethod,
                    acceptableMediaType: externalActionBase.AcceptedMediaType);
            }
            else
            {
                resolvedRoute = this.routeResolver.ActionToRoute(hypermediaObject, actionBase);
            }

            AddGeneralSirenActionProperties(jAction, property, resolvedRoute);
            AddActionParameters(actionBase, jAction, resolvedRoute.AcceptableMediaType);

            jActions.Add(jAction);
        }

        private void AddGeneralSirenActionProperties(
            JObject jAction,
            PropertyInfo property,
            ResolvedRoute resolvedRoute)
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

            jAction.Add("method", resolvedRoute.HttpMethod ?? "Undefined"); //TODO get method from rout resolver
            jAction.Add("href", resolvedRoute.Url);
        }

        private static HypermediaActionAttribute? GetHypermediaActionAttribute(
            PropertyInfo hypermediaActionPropertyInfo)
        {
            return hypermediaActionPropertyInfo.GetCustomAttribute<HypermediaActionAttribute>();
        }

        private void AddActionParameters(HypermediaActionBase hypermediaAction, JObject jAction,
            string? acceptedMediaType)
        {
            string classField;
            if (hypermediaAction is IFileUploadConfiguration fileUploadCommand)
            {
                jAction.Add("type", acceptedMediaType ?? DefaultMediaTypes.MultipartFormData);
                var fields = new JArray
                {
                    GetFileUploadActionDescription(fileUploadCommand.FileUploadConfiguration)
                };
                if (hypermediaAction.TryGetParameterType(out var parameterType))
                {
                    classField = ActionClasses.FileUploadActionWithParameterClass;
                    fields.Add(GetJsonParameterActionDescription(hypermediaAction, parameterType));
                }
                else
                {
                    classField = ActionClasses.FileUploadActionClass;
                }

                jAction.Add("fields", fields);
            }
            else
            {
                if (hypermediaAction.TryGetParameterType(out var parameterType))
                {
                    jAction.Add("type", acceptedMediaType ?? DefaultMediaTypes.ApplicationJson);
                    classField = ActionClasses.ParameterActionClass;
                    jAction.Add("fields", new JArray
                    {
                        GetJsonParameterActionDescription(hypermediaAction, parameterType),
                    });
                }
                else
                {
                    classField = ActionClasses.ParameterLessActionClass;
                }
            }

            jAction.Add("class", new JArray { classField });
        }

        private JObject GetFileUploadActionDescription(FileUploadConfiguration fileUploadConfiguration)
        {
            var jField = new JObject
            {
                { "name", "UploadFiles" },
                { "type", "file" },
            };

            if (fileUploadConfiguration.Accept.Any())
            {
                jField.Add(new JProperty("accept", string.Join(",", fileUploadConfiguration.Accept)));
            }

            if (fileUploadConfiguration.MaxFileSizeBytes >= 0)
            {
                jField.Add(new JProperty("maxFileSizeBytes", fileUploadConfiguration.MaxFileSizeBytes));
            }

            if (fileUploadConfiguration.AllowMultiple)
            {
                jField.Add(new JProperty("allowMultiple", true));
            }

            return jField;
        }

        private JObject GetJsonParameterActionDescription(HypermediaActionBase hypermediaAction, Type parameterType)
        {
            var jField = new JObject
            {
                { "name", parameterType.BeautifulName() },
                { "type", DefaultMediaTypes.ApplicationJson }
            };

            var routeKeysFromAction = GetRouteKeysIfActionHasSchemaParameters(hypermediaAction);
            routeResolver.TryGetRouteByType(parameterType, routeKeysFromAction).Match(
                some: classRoute => 
                {
                    jField.Add("class", new JArray { classRoute.Url });
                },
                none: () =>
                {
                    var generatedRouteUrl = routeResolver.RouteUrl(
                        RouteNames.ActionParameterTypes,
                        new { parameterTypeName = parameterType.BeautifulName() });
                    jField.Add("class", new JArray { generatedRouteUrl });
                });

            AddPrefilledValue(jField, hypermediaAction);

            return jField;
        }

        private object? GetRouteKeysIfActionHasSchemaParameters(HypermediaActionBase hypermediaAction)
        {
            if (hypermediaAction is IDynamicSchema dynamicSchema)
            {
                return dynamicSchema.SchemaRouteKeys;
            }

            return null;
        }

        private void AddPrefilledValue(JObject jField, HypermediaActionBase hypermediaAction)
        {
            var prefilledParameter = hypermediaAction.GetPrefilledParameter();
            if (prefilledParameter == null)
            {
                return;
            }

            if (prefilledParameter is string value)
            {
                // dynamic actions can pass a a string already. When migrating to system.text.json we can also allow JsonElement here in the future
                try
                {
                    jField.Add("value", JObject.Parse(value));
                }
                catch (Exception e)
                {
                    throw new AggregateException(
                        $"Can not read prefilled value from given string. Must be a valid JSON object.\nProvided string: {value}\n",
                        e);
                }
            }
            else
            {
                jField.Add("value", SerializeObjectProperties(prefilledParameter));
            }
        }

        private static bool IsHypermediaAction(PropertyInfo property)
        {
            return HypermediaActionBaseType.IsAssignableFrom(property.PropertyType);
        }

        private void SirenAddEntities(IHypermediaObject hypermediaObject, JObject sirenJson)
        {
            var embeddedEntities = GetAllEntities<IEmbeddedEntity>(hypermediaObject);
            var jEntities = new JArray();

            foreach (var (embeddedEntity, relations) in embeddedEntities)
            {
                if (embeddedEntity.Reference.IsResolved())
                {
                    var entitySiren = CreateSirenInternal(embeddedEntity.Reference.GetInstance()!, true,
                        relations);
                    jEntities.Add(entitySiren);
                }
                else
                {
                    var jLink = new JObject();

                    string resolvedAddress;
                    if (embeddedEntity.Reference is HypermediaExternalObjectReference externalReference)
                    {
                        AddClasses("External", jLink, externalReference.Classes);
                        resolvedAddress = externalReference.Uri.ToString();
                    }
                    else
                    {
                        var entityType = embeddedEntity.Reference.GetHypermediaType();
                        var hypermediaObjectAttribute = GetHypermediaObjectAttribute(entityType);
                        AddClasses(entityType, jLink, hypermediaObjectAttribute);
                        var (resolvedRoute, availableMediaTypes) = ResolveReferenceRoute(embeddedEntity.Reference);
                        resolvedAddress = resolvedRoute;
                        if (availableMediaTypes.Any())
                        {
                            throw new Exception("Embedded entities can not have media types");
                        }
                    }

                    AddEmbeddedEntityRelations(jLink, relations);
                    jLink.Add("href", resolvedAddress);
                    jEntities.Add(jLink);
                }
            }

            sirenJson.Add("entities", jEntities);
        }

        private void AddLinks(IHypermediaObject hypermediaObject, JObject sirenJson)
        {
            var hypermediaLinks = GetAllEntities<ILink>(hypermediaObject);
            var dictionary = new Dictionary<IReadOnlyCollection<string>, HypermediaObjectReferenceBase>(new StringReadOnlyCollectionComparer());
            foreach (var (link, rel) in hypermediaLinks)
            {
                dictionary[rel] = link.Reference;
            }
            var jLinks = new JArray();

            foreach (var hypermediaLink in dictionary)
            {
                var jLink = new JObject();

                var jRel = new JArray { hypermediaLink.Key };
                jLink.Add("rel", jRel);

                var (resolvedRoute, availableMediaTypes) = ResolveReferenceRoute(hypermediaLink.Value);

                jLink.Add("href", resolvedRoute);

                if (availableMediaTypes.Any())
                {
                    jLink.Add("type", availableMediaTypes);
                }

                // todo also add classes
                jLinks.Add(jLink);
            }

            sirenJson.Add("links", jLinks);
        }

        private void AssertNoBaseRelatedEntities(IHypermediaObject hypermediaObject)
        {
            var nonSpecificProperties = hypermediaObject.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(IsRelatedEntityProperty<IRelatedEntity>)
                .Where(p => !IsRelatedEntityProperty<ILink>(p))
                .Where(p => !IsRelatedEntityProperty<IEmbeddedEntity>(p))
                .ToList();
            if (nonSpecificProperties.Count > 0)
            {
                throw new HypermediaException(
                    $"Properties {string.Join(", ", nonSpecificProperties.Select(p => p.Name))} of type {hypermediaObject.GetType().BeautifulName()} are neither {nameof(ILink)} not {nameof(IEmbeddedEntity)}");
            }
        }

        private IEnumerable<(TRelatedEntity Entity, string[] Rel)> GetAllEntities<TRelatedEntity>(IHypermediaObject hypermediaObject)
            where TRelatedEntity : IRelatedEntity
        {
            var type = hypermediaObject.GetType();
            var properties = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetMethod is not null)
                .Where(IsRelatedEntityProperty<TRelatedEntity>)
                .SelectMany(property =>
                {
                    var attribute = property.GetCustomAttribute<RelationsAttribute>();
                    if (attribute is null)
                    {
                        throw new HypermediaException(
                            $"missing {nameof(RelationsAttribute)} on property {property.Name} of type {hypermediaObject.GetType().BeautifulName()}");
                    }
                    var result = property.GetMethod!.Invoke(hypermediaObject, []);
                    if (result is null)
                    {
                        return [];
                    }
                    if (AttributedRouteHelper.Is<TRelatedEntity>(property.PropertyType))
                    {
                        var singleEntity = (TRelatedEntity)result;
                        return [(singleEntity, attribute.Rel)];
                    }
                    else if (AttributedRouteHelper.Is<IEnumerable<TRelatedEntity>>(property.PropertyType))
                    {
                        var multiple = (IEnumerable<TRelatedEntity>)result;
                        return multiple.Select(m => (m, attribute.Rel));
                    }
                    else
                    {
                        Debug.Assert(false);
                        return [];
                    }
                });
            return properties;
        }

        private static bool IsRelatedEntityProperty<TRelatedEntity>(PropertyInfo p)
            where TRelatedEntity : IRelatedEntity
            => AttributedRouteHelper.Is<TRelatedEntity>(p.PropertyType) ||
               AttributedRouteHelper.Is<IEnumerable<TRelatedEntity>>(p.PropertyType);

        private Tuple<string, string> ResolveReferenceRoute(HypermediaObjectReferenceBase reference)
        {
            var resolvedRoute = routeResolver.ReferenceToRoute(reference);
            var query = reference.GetQuery();
            var buildRoute = resolvedRoute.Url + queryStringBuilder.CreateQueryString(query);

            var resolvedRouteAvailableMediaTypes = string.Join(",", resolvedRoute.AvailableMediaTypes);
            return new Tuple<string, string>(buildRoute, resolvedRouteAvailableMediaTypes);
        }

        private void AddProperties(IHypermediaObject hypermediaObject, JObject sirenJson)
        {
            sirenJson.Add("properties", SerializeObjectProperties(hypermediaObject));
        }

        private void AddProperty(object propertyObject, PropertyInfo publicProperty, JObject jProperties)
        {
            if (PropertyHasIgnoreAttribute(publicProperty))
            {
                return;
            }

            if (IsHypermediaAction(publicProperty))
            {
                return;
            }

            var propertyType = publicProperty.PropertyType;
            var propertyTypeInfo = propertyType.GetTypeInfo();

            var propertyName = GetPropertyName(publicProperty);
            var value = publicProperty.GetValue(propertyObject);

            var jValue = ValueToJToken(value, propertyType, propertyTypeInfo);
            if (jValue != null || this.configuration.WriteNullProperties)
            {
                jProperties.Add(propertyName, jValue);
            }
        }

        private JToken? ValueToJToken(object? value, Type propertyType, TypeInfo propertyTypeInfo)
        {
            if (value == null)
            {
                return null;
            }

            // enum is also a value type so check first
            if (propertyTypeInfo.IsEnum)
            {
                var enumAsString = EnumHelper.GetEnumMemberValue(propertyType, value);
                return new JValue(enumAsString);
            }

            // enum can be wrapped in a nullable
            if (IsNullableEnum(propertyType, out var enumType))
            {
                var enumAsString = EnumHelper.GetEnumMemberValue(enumType, value);
                return new JValue(enumAsString);
            }


            if (propertyTypeInfo.IsValueType)
            {
                return new JValue(value);
            }

            // string is also enumerable so check first
            if (propertyType == typeof(string))
            {
                return new JValue(string.Format(CultureInfo.InvariantCulture, "{0}", value));
            }

            if (IsTimeType(propertyType))
            {
                return new JValue(((IFormattable)value).ToString("o", CultureInfo.InvariantCulture));
            }

            if (IsContainerTypeForString(propertyType))
            {
                return new JValue(value.ToString());
            }

            if (IsIEnumerable(value, propertyType, out var iEnumerable))
            {
                return SerializeEnumerable(iEnumerable);
            }

            if (propertyType == typeof(Type))
            {
                return ((Type)value).FullName;
            }

            // check last so special types can be handled first
            if (propertyTypeInfo.IsClass)
            {
                return SerializeObjectProperties(value);
            }

            throw new HypermediaFormatterException(
                $"Can not serialize type: {propertyType.BeautifulName()} value: {value}");
        }

        private static bool IsNullableEnum(Type nullableType, [NotNullWhen(true)] out Type? enumType)
        {
            var underlyingType = Nullable.GetUnderlyingType(nullableType);
            if (underlyingType != null && underlyingType.GetTypeInfo().IsEnum)
            {
                enumType = underlyingType;
                return true;
            }

            enumType = null;
            return false;
        }


        static bool IsContainerTypeForString(Type propertyType)
        {
            return propertyType == typeof(Uri);
        }

        private JObject SerializeObjectProperties(object propertyObject)
        {
            var type = propertyObject.GetType();
            var publicProperties = type.GetTypeInfo()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => !IsRelatedEntityProperty<IRelatedEntity>(p));

            var jProperties = new JObject();
            foreach (var publicProperty in publicProperties)
            {
                AddProperty(propertyObject, publicProperty, jProperties);
            }

            return jProperties;
        }

        private JToken SerializeEnumerable(IEnumerable iEnumerable)
        {
            var enumerableType = iEnumerable.GetType();

            var itemType = enumerableType.IsArray
                ? enumerableType.GetElementType()!
                : enumerableType.GenericTypeArguments.Single();
            var itemTypeInfo = itemType.GetTypeInfo();

            // check for polymorphic serialization which is solved in net 6 and lower by using object as item type
            // in this case we need to get the type every time
            var getTypeForEachItem = itemType == typeof(object);

            var result = new JArray();
            foreach (var item in iEnumerable)
            {
                if (item == null)
                {
                    result.Add(JValue.CreateNull());
                    continue;
                }

                if (getTypeForEachItem)
                {
                    itemType = item.GetType();
                    itemTypeInfo = itemType.GetTypeInfo();
                }

                result.Add(ValueToJToken(item, itemType, itemTypeInfo));
            }

            return result;
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

        private static bool IsIEnumerable(object publicProperty, Type propertyType,
            [NotNullWhen(true)] out IEnumerable? iEnumerable)
        {
            if (propertyType.GetTypeInfo().GetInterfaces().Contains(typeof(IEnumerable)))
            {
                iEnumerable = (IEnumerable)publicProperty;
                return true;
            }

            iEnumerable = null;
            return false;
        }

        private static bool PropertyHasIgnoreAttribute(PropertyInfo publicProperty)
        {
            return publicProperty.CustomAttributes.Any(a =>
                a.AttributeType == typeof(FormatterIgnoreHypermediaPropertyAttribute));
        }

        private static void AddClasses(IHypermediaObject hypermediaObject, JObject sirenJson,
            HypermediaObjectAttribute? hypermediaObjectAttribute)
        {
            var hmoType = hypermediaObject.GetType();
            AddClasses(hmoType, sirenJson, hypermediaObjectAttribute);
        }

        static void AddClasses(Type hmoType, JObject sirenJson, HypermediaObjectAttribute? hypermediaObjectAttribute)
        {
            AddClasses(hmoType.BeautifulName(), sirenJson, hypermediaObjectAttribute?.Classes);
        }

        private static void AddClasses(string defaultClass, JObject sirenJson, IEnumerable<string>? classes)
        {
            var sirenClasses = new JArray();

            if (classes != null)
            {
                foreach (var hypermediaClass in classes)
                {
                    sirenClasses.Add(hypermediaClass);
                }
            }
            else
            {
                var hypermediaObjectName = defaultClass;
                sirenClasses.Add(hypermediaObjectName);
            }

            sirenJson.Add("class", sirenClasses);
        }

        private static void AddEmbeddedEntityRelations(JObject jEmbeddedEntity,
            IReadOnlyCollection<string> embeddedEntityRelations)
        {
            var rels = new JArray();
            foreach (var embeddedEntityRelation in embeddedEntityRelations)
            {
                rels.Add(embeddedEntityRelation);
            }

            jEmbeddedEntity.Add("rel", rels);
        }

        private static void AddTitle(JObject sirenJson, HypermediaObjectAttribute? hypermediaObjectAttribute)
        {
            if (!string.IsNullOrEmpty(hypermediaObjectAttribute?.Title))
            {
                sirenJson.Add("title", hypermediaObjectAttribute.Title);
            }
        }
    }
}