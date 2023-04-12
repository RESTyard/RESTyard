using System;
using System.Collections;
using System.Collections.Generic;
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
using RESTyard.AspNetCore.WebApi.ExtensionMethods;
using RESTyard.AspNetCore.WebApi.RouteResolver;
using RESTyard.MediaTypes;

namespace RESTyard.AspNetCore.WebApi.Formatter
{
    public class SirenConverter : IHypermediaJsonConverter, IHypermediaConverter
    {
        private static readonly HypermediaConverterConfiguration DefaultConfiguration = new HypermediaConverterConfiguration();

        private readonly IQueryStringBuilder queryStringBuilder;
        private readonly IHypermediaRouteResolver routeResolver;
        private readonly HypermediaConverterConfiguration configuration;
        private static readonly Type HypermediaActionBaseType = typeof(HypermediaActionBase).GetTypeInfo();

        public SirenConverter(IHypermediaRouteResolver routeResolver, IQueryStringBuilder queryStringBuilder, HypermediaConverterConfiguration configuration = null)
        {
            this.queryStringBuilder = queryStringBuilder;
            this.routeResolver = routeResolver;

            this.configuration = configuration ?? DefaultConfiguration;
        }

        public string ConvertToString(HypermediaObject hypermediaObject)
        {
            return ConvertToJson(hypermediaObject).ToString();
        }

        public JObject ConvertToJson(HypermediaObject hypermediaObject)
        {
            return CreateSirenInternal(hypermediaObject);
        }

        private JObject CreateSirenInternal(HypermediaObject hypermediaObject, bool isEmbedded = false, IReadOnlyCollection<string> embeddedEntityRelations = null)
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
                var actionBase = (HypermediaActionBase)action;

                if (actionBase.CanExecute())
                {
                    AddActionSirenContent(hypermediaObject, action, actionBase, property, jActions);
                }
            }

            sirenJson.Add("actions", jActions);
        }

        private void AddActionSirenContent(HypermediaObject hypermediaObject, object action, HypermediaActionBase actionBase, PropertyInfo property, JArray jActions)
        {
            var jAction = new JObject();
            ResolvedRoute resolvedRoute;
            if (action is HypermediaExternalActionBase externalActionBase)
            {
                resolvedRoute = new ResolvedRoute(externalActionBase.ExternalUri.ToString(), externalActionBase.HttpMethod, acceptableMediaType: externalActionBase.AcceptedMediaType);
            } else
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

            jAction.Add("method", resolvedRoute.HttpMethod.ToString());//TODO get method from rout resolver
            jAction.Add("href", resolvedRoute.Url);
        }

        private static HypermediaActionAttribute GetHypermediaActionAttribute(PropertyInfo hypermediaActionPropertyInfo)
        {
            return hypermediaActionPropertyInfo.GetCustomAttribute<HypermediaActionAttribute>();
        }

        private void AddActionParameters(HypermediaActionBase hypermediaAction, JObject jAction, string acceptedMediaType)
        {
            if (!hypermediaAction.HasParameter())
            {
                jAction.Add("class", new JArray { ActionClasses.ParameterLessActionClass });
                return;
            }
            
            jAction.Add("type",  acceptedMediaType ?? DefaultMediaTypes.ApplicationJson);
            AddActionFields(jAction, hypermediaAction);
        }

        private void AddActionFields(JObject jAction, HypermediaActionBase hypermediaAction)
        {
            var parameterType = hypermediaAction.ParameterType();

            if (parameterType == typeof(FileUploadConfiguration))
            {
                AddFileUploadActionDescription(jAction, hypermediaAction);
            } else
            {
                AddJsonParameterActionDescription(jAction, hypermediaAction, parameterType);    
            }
        }

        private void AddFileUploadActionDescription(JObject jAction, HypermediaActionBase hypermediaAction)
        {
            const string InputTypeFile = "file";
            
            if (hypermediaAction is not FileUploadHypermediaAction fileUploadAction)
            {
                throw new Exception($"Parameter indicates file upload action but action type does not match: {hypermediaAction.GetType()}. Can not create action description.");
            }

            var fileUploadConfiguration = fileUploadAction.FileUploadConfiguration;
            var jfield = new JObject
            {
                { "name", "UploadFiles"},
                { "type", InputTypeFile }
            };

            if (fileUploadConfiguration.Accept.Any())
            {
                jfield.Add(new JProperty("accept", string.Join(", ", fileUploadConfiguration.Accept)));
            }

            if (fileUploadConfiguration.MaxFileSizeBytes >= 0)
            {
                jfield.Add(new JProperty("maxfilesizebytes", fileUploadConfiguration.MaxFileSizeBytes));
            }
            
            jAction.Add("class", new JArray { ActionClasses.FileUploadActionClass });
            jAction.Add("fields", new JArray { jfield });
        }

        private void AddJsonParameterActionDescription(JObject jAction, HypermediaActionBase hypermediaAction, Type parameterType)
        {
            jAction.Add("class", new JArray { ActionClasses.ParameterActionClass });
            
            var jfield = new JObject
            {
                { "name", parameterType.BeautifulName() },
                { "type", DefaultMediaTypes.ApplicationJson }
            };

            if (!routeResolver.TryGetRouteByType(parameterType, out var classRoute))
            {
                var generatedRouteUrl = routeResolver.RouteUrl(RouteNames.ActionParameterTypes,
                    new { parameterTypeName = parameterType.BeautifulName() });
                jfield.Add("class", new JArray { generatedRouteUrl });
            }
            else
            {
                jfield.Add("class", new JArray { classRoute.Url });
            }

            AddPrefilledValue(jfield, hypermediaAction);
            
            jAction.Add("fields", new JArray { jfield });
        }

        private void AddPrefilledValue(JObject jfield, HypermediaActionBase hypermediaAction)
        {
            var prefilledParameter = hypermediaAction.GetPrefilledParameter();
            if (prefilledParameter == null)
            {
                return;
            }
            
            jfield.Add("value", SerializeObjectProperties(prefilledParameter));
        }

        private static bool IsHypermediaAction(PropertyInfo property)
        {
            return HypermediaActionBaseType.IsAssignableFrom(property.PropertyType);
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
                        var (resolvedRoute, avaialbleMediaTypes) = ResolveReferenceRoute(embeddedEntity.Reference);
                        resolvedAddress = resolvedRoute;
                        if (avaialbleMediaTypes.Any())
                        {
                            throw new Exception("Embedded entities can not have media types");
                        }
                    }

                    AddEmbeddedEntityRelations(jLink, embeddedEntity.Relations);
                    jLink.Add("href", resolvedAddress);
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

                var (resolvedRoute, avaialbleMediaTypes) = ResolveReferenceRoute(hypermediaLink.Value.Reference);

                jLink.Add("href", resolvedRoute);

                if (avaialbleMediaTypes.Any())
                {
                    jLink.Add("type", avaialbleMediaTypes);
                }
                // todo also add classes
                jLinks.Add(jLink);
            }

            sirenJson.Add("links", jLinks);
        }

        private Tuple<string, string> ResolveReferenceRoute(HypermediaObjectReferenceBase reference)
        {
            var resolvedRoute = routeResolver.ReferenceToRoute(reference);
            var query = reference.GetQuery();
            var buildRoute = resolvedRoute.Url + queryStringBuilder.CreateQueryString(query);

            var resolvedRouteAvailableMediaTypes = string.Join(",", resolvedRoute.AvailableMediaTypes);
            return new Tuple<string, string>(buildRoute, resolvedRouteAvailableMediaTypes);
        }
        
        private void AddProperties(HypermediaObject hypermediaObject, JObject sirenJson)
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

            var jvalue = ValueToJToken(value, propertyType, propertyTypeInfo);
            if (jvalue != null || this.configuration.WriteNullProperties)
            {
                jProperties.Add(propertyName, jvalue);
            }
        }

        private JToken ValueToJToken(object value, Type propertyType, TypeInfo propertyTypeInfo)
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

            if (IsIEnumerable(value, propertyType, out var ienumerable))
            {
                return SerializeEnumerable(ienumerable);
            }

            if ( propertyType == typeof(Type))
            {
                return ((Type)value).FullName;
            }

            // check last so special types can be handled first
            if (propertyTypeInfo.IsClass)
            {
                return SerializeObjectProperties(value);
            }

            throw new HypermediaFormatterException($"Can not serialize type: {propertyType.BeautifulName()} value: {value}");
        }

        private static bool IsNullableEnum(Type nullableType, out Type enumType)
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
            var publicProperties = type.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var jProperties = new JObject();
            foreach (var publicProperty in publicProperties)
            {
                AddProperty(propertyObject, publicProperty, jProperties);
            }

            return jProperties;
        }

        private JToken SerializeEnumerable(IEnumerable ienumerable)
        {
            var enumerableType = ienumerable.GetType();

            var itemType = enumerableType.IsArray ? enumerableType.GetElementType() : enumerableType.GenericTypeArguments.Single();
            var itemTypeInfo = itemType.GetTypeInfo();
            
            // check for polymorphic serialization which is solved in net 6 and lower by using object as item type
            // in this case we need to get the type every time
            var getTypeForEachItem = itemType == typeof(object);

            var result = new JArray();
            foreach (var item in ienumerable)
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

        private static bool IsIEnumerable(object publicProperty, Type propertyType, out IEnumerable iEnumerable)
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
            return publicProperty.CustomAttributes.Any(a => a.AttributeType == typeof(FormatterIgnoreHypermediaPropertyAttribute));
        }

        private static void AddClasses(HypermediaObject hypermediaObject, JObject sirenJson, HypermediaObjectAttribute hypermediaObjectAttribute)
        {
            var hmoType = hypermediaObject.GetType();
            AddClasses(hmoType, sirenJson, hypermediaObjectAttribute);
        }

        static void AddClasses(Type hmoType, JObject sirenJson, HypermediaObjectAttribute hypermediaObjectAttribute)
        {
            AddClasses(hmoType.BeautifulName(), sirenJson, hypermediaObjectAttribute?.Classes);
        }

        private static void AddClasses(string defaultClass, JObject sirenJson, IEnumerable<string> classes)
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

        private static void AddEmbeddedEntityRelations(JObject jembeddedEntity, IReadOnlyCollection<string> embeddedEntityRelations)
        {
            var rels = new JArray();
            foreach (var embeddedEntityRelation in embeddedEntityRelations)
            {
                rels.Add(embeddedEntityRelation);
            }
            jembeddedEntity.Add("rel", rels);
        }

        private static void AddTitle(JObject sirenJson, HypermediaObjectAttribute hypermediaObjectAttribute)
        {
            if (!string.IsNullOrEmpty(hypermediaObjectAttribute?.Title))
            {
                sirenJson.Add("title", hypermediaObjectAttribute.Title);
            }
        }
    }
}