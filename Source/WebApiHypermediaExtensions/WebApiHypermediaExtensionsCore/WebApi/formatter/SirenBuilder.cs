using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using WebApiHypermediaExtensionsCore.Exceptions;
using WebApiHypermediaExtensionsCore.Hypermedia;
using WebApiHypermediaExtensionsCore.Hypermedia.Actions;
using WebApiHypermediaExtensionsCore.Hypermedia.Attributes;
using WebApiHypermediaExtensionsCore.Hypermedia.Links;
using WebApiHypermediaExtensionsCore.Query;
using WebApiHypermediaExtensionsCore.Responses;
using WebApiHypermediaExtensionsCore.Util.Enum;
using WebApiHypermediaExtensionsCore.WebApi.RouteResolver;

namespace WebApiHypermediaExtensionsCore.WebApi.Formatter
{
    public class SirenBuilder : ISirenBuilder
    {
        public string CreateSiren(HypermediaObject hypermediaObject, IHypermediaRouteResolver routeResolver, IQueryStringBuilder queryStringBuilder)
        {
            return CreateSirenInternal(routeResolver, hypermediaObject, queryStringBuilder).ToString();
        }

        public JObject CreateSirenJObject(HypermediaObject hypermediaObject, IHypermediaRouteResolver routeResolver, IQueryStringBuilder queryStringBuilder)
        {
            return CreateSirenInternal(routeResolver, hypermediaObject, queryStringBuilder);
        }

        private static JObject CreateSirenInternal(IHypermediaRouteResolver routeResolver, HypermediaObject hypermediaObject, IQueryStringBuilder queryStringBuilder, bool isEmbedded = false, List<string> embeddedEntityRelations = null)
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
            SirenAddEntities(routeResolver, hypermediaObject, sirenJson, queryStringBuilder);
            AddActions(routeResolver, hypermediaObject, sirenJson);


            AddLinks(routeResolver, hypermediaObject, sirenJson, queryStringBuilder);

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

        private static void AddActions(IHypermediaRouteResolver routeResolver, HypermediaObject hypermediaObject, JObject sirenJson)
        {
            var properties = hypermediaObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

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
                    AddGeneralSirenActionProperties(jAction, property, routeResolver, hypermediaObject, actionBase);

                    AddActionParameters(routeResolver, actionBase, jAction);

                    jActions.Add(jAction);
                }
            }

            sirenJson.Add("actions", jActions);
        }

        private static void AddGeneralSirenActionProperties(JObject jAction, PropertyInfo property, IHypermediaRouteResolver routeResolver, HypermediaObject hypermediaObject, HypermediaActionBase actionBase)
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

        private static void AddActionParameters(IHypermediaRouteResolver routeResolver, HypermediaActionBase hypermediaAction, JObject jAction)
        {
            if (!hypermediaAction.HasParameter())
            {
                return;
            }

            jAction.Add("type", DefaultContentTypes.ApplicationJson);
            AddActionFields(routeResolver, jAction, hypermediaAction.ParameterType());
        }

        private static void AddActionFields(IHypermediaRouteResolver routeResolver, JObject jAction, Type actionParameterType)
        {
            var jfield = new JObject();
            jfield.Add("name", actionParameterType.Name);
            jfield.Add("type", DefaultContentTypes.ApplicationJson);

            string classRoute;
            try
            {
                classRoute = routeResolver.TypeToRoute(actionParameterType);

            }
            catch (Exception)
            {
                classRoute = actionParameterType.Name;
            }

            jfield.Add("class", classRoute);

            var jFields = new JArray();
            jFields.Add(jfield);
            jAction.Add("fields", jFields);
        }

        private static bool IsHypermediaAction(PropertyInfo property)
        {
            return typeof(HypermediaActionBase).IsAssignableFrom(property.PropertyType);
        }

        private static void SirenAddEntities(IHypermediaRouteResolver routeResolver, HypermediaObject hypermediaObject, JObject sirenJson, IQueryStringBuilder queryStringBuilder)
        {
            var embeddedEntities = hypermediaObject.Entities;
            var jEntities = new JArray();

            foreach (var embeddedEntity in embeddedEntities)
            {
                var referenceType = embeddedEntity.Reference.GetType();

                if (typeof(HypermediaObjectKeyReference).IsAssignableFrom(referenceType)
                    || typeof(HypermediaObjectQueryReference).IsAssignableFrom(referenceType))
                {
                    var jLink = new JObject();

                    var entityType = embeddedEntity.Reference.GetHypermediaType();
                    var hypermediaObjectAttribute = GetHypermediaObjectAttribute(entityType);
                    AddClasses(entityType, jLink, hypermediaObjectAttribute);
                    AddEmbeddedEntityRelations(jLink, embeddedEntity.Relations);

                    var resolvedAdress = ResolvedKeyOrQueryReference(routeResolver, embeddedEntity.Reference, queryStringBuilder);
                    jLink.Add("href", resolvedAdress);
                    jEntities.Add(jLink);
                }
                else if (typeof(HypermediaObjectReference).IsAssignableFrom(referenceType))
                {
                    var entitySiren = CreateSirenInternal(routeResolver, embeddedEntity.Reference.Resolve(), queryStringBuilder, true, embeddedEntity.Relations);
                    jEntities.Add(entitySiren);
                }
                else
                {
                    throw new HypermediaFormatterException("Unknown reference type for embedded entity.");
                }
            }

            sirenJson.Add("entities", jEntities);
        }

        private static void AddLinks(IHypermediaRouteResolver routeResolver, HypermediaObject hypermediaObject, JObject sirenJson, IQueryStringBuilder queryStringBuilder)
        {
            var hypermediaLinks = hypermediaObject.Links;
            var jLinks = new JArray();

            foreach (var hypermediaLink in hypermediaLinks)
            {
                var jLink = new JObject();

                var jRel = new JArray();
                jRel.Add(hypermediaLink.Key);
                jLink.Add("rel", jRel);

                var resolvedAdress = ResolvedKeyOrQueryReference(routeResolver, hypermediaLink.Value, queryStringBuilder);

                jLink.Add("href", resolvedAdress);

                jLinks.Add(jLink);
            }

            sirenJson.Add("links", jLinks);
        }

        private static string ResolvedKeyOrQueryReference(IHypermediaRouteResolver routeResolver, HypermediaObjectReferenceBase reference, IQueryStringBuilder queryStringBuilder)
        {
            var resolvedAdress = routeResolver.ReferenceToRoute(reference);
            var hypermediaObjectQueryReference = reference as HypermediaObjectQueryReference;
            if (hypermediaObjectQueryReference != null)
            {
                resolvedAdress += queryStringBuilder.CreateQueryString(hypermediaObjectQueryReference.Query);
            }
            return resolvedAdress;
        }

        private static void AddProperties(HypermediaObject hypermediaObject, JObject sirenJson)
        {
            var type = hypermediaObject.GetType();
            var publicProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var jProperties = new JObject();
            foreach (var publicProperty in publicProperties)
            {
                if (PropertyHasIgnoreAttribute(publicProperty))
                {
                    continue;
                }
                if (IsHypermediaAction(publicProperty))
                {
                    continue;
                }

                var propertyType = publicProperty.PropertyType;
                var propertyTypeInfo = propertyType.GetTypeInfo();
                if (propertyTypeInfo.IsClass && propertyType != typeof(string))
                {
                    continue;
                }

                var hypermediaPropertyAttribute = GetHypermediaPropertyAttribute(publicProperty);

                string propertyName;
                if (!string.IsNullOrEmpty(hypermediaPropertyAttribute?.Name))
                {
                    propertyName = hypermediaPropertyAttribute.Name;
                }
                else
                {
                    propertyName = publicProperty.Name;
                }

                var value = publicProperty.GetValue(hypermediaObject);
                if (value == null)
                {
                    jProperties.Add(propertyName, null);
                }
                else
                {
                    
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
                        
                    }
                    else if (IsTimeType(propertyType))
                    {
                        jProperties.Add(propertyName, ((IFormattable)value).ToString("o", CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        jProperties.Add(propertyName, string.Format(CultureInfo.InvariantCulture, "{0}", value));
                    }

                }
            }

            sirenJson.Add("properties", jProperties);
        }

        public static bool IsTimeType(Type type)
        {
            return type == typeof(DateTime) || type == typeof(DateTimeOffset);
        }
       
        private static bool PropertyHasIgnoreAttribute(PropertyInfo publicProperty)
        {
            return publicProperty.CustomAttributes.Any(a => a.AttributeType == typeof(FormatterIgnoreHypermediaPropertyAttribute));
        }

        private static void AddClasses(HypermediaObject hypermediaObject, JObject sirenJson, HypermediaObjectAttribute hypermediaObjectAttribute)
        {
            AddClasses(hypermediaObject.GetType(), sirenJson, hypermediaObjectAttribute);
        }

        private static void AddClasses(Type hypermediaObjectType, JObject sirenJson, HypermediaObjectAttribute hypermediaObjectAttribute)
        {
            var sirenClasses = new JArray();

            if (hypermediaObjectAttribute?.Classes != null)
            {
                foreach (var hypermediaClass in hypermediaObjectAttribute.Classes)
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

        private static void AddTitle(JObject sirenJson, HypermediaObjectAttribute hypermediaObjectAttribute)
        {
            if (!string.IsNullOrEmpty(hypermediaObjectAttribute?.Title))
            {
                sirenJson.Add("title", hypermediaObjectAttribute.Title);
            }
        }
    }
}