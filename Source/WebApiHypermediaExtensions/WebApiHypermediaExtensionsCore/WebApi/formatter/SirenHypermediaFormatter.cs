using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json.Linq;
using WebApiHypermediaExtensionsCore.ErrorHandling;
using WebApiHypermediaExtensionsCore.Exceptions;
using WebApiHypermediaExtensionsCore.Hypermedia;
using WebApiHypermediaExtensionsCore.Hypermedia.Actions;
using WebApiHypermediaExtensionsCore.Hypermedia.Attributes;
using WebApiHypermediaExtensionsCore.Hypermedia.Links;
using WebApiHypermediaExtensionsCore.Query;
using WebApiHypermediaExtensionsCore.Responses;
using WebApiHypermediaExtensionsCore.WebApi.RouteResolver;

namespace WebApiHypermediaExtensionsCore.WebApi.Formatter
{
    public class SirenHypermediaFormatter : IOutputFormatter
    {
        private readonly IRouteResolverFactory routeResolverFactory;
        private readonly IRouteKeyFactory routeKeyFactory;
        private readonly IQueryStringBuilder queryStringBuilder;

        public SirenHypermediaFormatter(IRouteResolverFactory routeResolverFactory, IRouteKeyFactory routeKeyFactory, IQueryStringBuilder queryStringBuilder)
        {

            this.routeResolverFactory = routeResolverFactory;
            this.routeKeyFactory = routeKeyFactory;
            this.queryStringBuilder = queryStringBuilder;
        }

        public bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            if (!(context.Object is HypermediaObject))
            {
                return false;
            }

            var contentType = context.ContentType.ToString();
            if (string.IsNullOrEmpty(contentType))
            {
                return true;
            }

            if (contentType.Contains(DefaultContentTypes.Siren))
            {
                return true;
            }

            return false;
        }

        public async Task WriteAsync(OutputFormatterWriteContext context)
        {
            var hypermediaObject = context.Object as HypermediaObject;
            if (hypermediaObject == null)
            {
                throw new HypermediaFormatterException("Formatter expected a HypermediaObject but is not.");
            }

            var response = context.HttpContext.Response;

            var urlHelper = FormatterHelper.GetUrlHelperForCurrentContext(context);
            var routeResolver = routeResolverFactory.CreateRouteResolver(urlHelper, routeKeyFactory);

            var sirenJson = CreateSiren(routeResolver, hypermediaObject);
            response.ContentType = DefaultContentTypes.Siren;
            await WriteToBody(context, response, sirenJson.ToString());
        }

        private static async Task WriteToBody(OutputFormatterWriteContext context, HttpResponse response, string content)
        {
            using (var writer = context.WriterFactory(response.Body, Encoding.UTF8))
            {
                await writer.WriteAsync(content);
                await writer.FlushAsync();
            }
        }

        public JObject CreateSiren(IHypermediaRouteResolver routeResolver, HypermediaObject hypermediaObject, bool isEmbedded = false, List<string> embeddedEntityRelations = null)
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

            if (!isEmbedded)
            {
                SirenAddEntities(routeResolver, hypermediaObject, sirenJson);
                AddActions(routeResolver, hypermediaObject, sirenJson);
            }
            

            AddLinks(routeResolver, hypermediaObject, sirenJson);

            return sirenJson;
        }

        private HypermediaObjectAttribute GetHypermediaObjectAttribute(HypermediaObject hypermediaObject)
        {
            return GetHypermediaObjectAttribute(hypermediaObject.GetType());
        }

        private HypermediaObjectAttribute GetHypermediaObjectAttribute(Type hypermediaObjectType)
        {
            return hypermediaObjectType.GetTypeInfo().GetCustomAttribute<HypermediaObjectAttribute>();
        }

        private static HypermediaPropertyAttribute GetHypermediaPropertyAttribute(PropertyInfo hypermediaPropertyInfo)
        {
            return hypermediaPropertyInfo.GetCustomAttribute<HypermediaPropertyAttribute>();
        }

        private void AddActions(IHypermediaRouteResolver routeResolver, HypermediaObject hypermediaObject, JObject sirenJson)
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
            if (hypermediaActionAttribute != null && !string.IsNullOrEmpty(hypermediaActionAttribute.Name))
            {
                actionName = hypermediaActionAttribute.Name;
            }
            else
            {
                actionName = property.Name;
            }
            jAction.Add("name", actionName);

            if (hypermediaActionAttribute != null && !string.IsNullOrEmpty(hypermediaActionAttribute.Title))
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

        private void AddActionParameters(IHypermediaRouteResolver routeResolver, HypermediaActionBase hypermediaAction, JObject jAction)
        {
            if (!hypermediaAction.HasParameter())
            {
                return;
            }

            jAction.Add("type", DefaultContentTypes.ApplicationJson);
            AddActionFields(routeResolver, jAction, hypermediaAction.ParameterType());
        }

        private void AddActionFields(IHypermediaRouteResolver routeResolver, JObject jAction, Type actionParameterType)
        {
            var jfield = new JObject();
            jfield.Add("name", actionParameterType.Name);
            jfield.Add("type", DefaultContentTypes.ApplicationJson);

            string classRoute;
            try
            {
                classRoute = routeResolver.ParameterTypeToRoute(actionParameterType);

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

        private void SirenAddEntities(IHypermediaRouteResolver routeResolver, HypermediaObject hypermediaObject, JObject sirenJson)
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

                    var resolvedAdress = ResolvedKeyOrQueryReference(routeResolver, embeddedEntity.Reference);
                    jLink.Add("href", resolvedAdress);
                    jEntities.Add(jLink);
                }
                else if (typeof(HypermediaObjectReference).IsAssignableFrom(referenceType))
                {
                    var entitySiren = CreateSiren(routeResolver, embeddedEntity.Reference.Resolve(), true, embeddedEntity.Relations);
                    jEntities.Add(entitySiren);
                }
                else
                {
                    throw new HypermediaFormatterException("Unknown reference type for embedded entity.");
                }
            }

            sirenJson.Add("entities", jEntities);
        }



        private void AddLinks(IHypermediaRouteResolver routeResolver, HypermediaObject hypermediaObject, JObject sirenJson)
        {
            var hypermediaLinks = hypermediaObject.Links;
            var jLinks = new JArray();

            foreach (var hypermediaLink in hypermediaLinks)
            {
                var jLink = new JObject();

                var jRel = new JArray();
                jRel.Add(hypermediaLink.Key);
                jLink.Add("rel", jRel);

                var resolvedAdress = ResolvedKeyOrQueryReference(routeResolver, hypermediaLink.Value);

                jLink.Add("href", resolvedAdress);

                jLinks.Add(jLink);
            }

            sirenJson.Add("links", jLinks);
        }

        private string ResolvedKeyOrQueryReference(IHypermediaRouteResolver routeResolver, HypermediaObjectReferenceBase reference)
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
                    jProperties.Add(propertyName, "null");
                }
                else
                {
                    jProperties.Add(propertyName, value.ToString());
                }
            }

            sirenJson.Add("properties", jProperties);
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

            if (hypermediaObjectAttribute != null && hypermediaObjectAttribute.Classes != null)
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

        private void AddEmbeddedEntityRelations(JObject jembeddedEntity, List<string> embeddedEntityRelations)
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
            if (hypermediaObjectAttribute != null && !string.IsNullOrEmpty(hypermediaObjectAttribute.Title))
            {
                sirenJson.Add("title", hypermediaObjectAttribute.Title);
            }
        }
    }
}