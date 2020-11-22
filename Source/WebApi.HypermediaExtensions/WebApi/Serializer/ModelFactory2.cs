using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Bluehands.Hypermedia.Model;
using Bluehands.Hypermedia.Relations;
using FunicularSwitch;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;
using WebApi.HypermediaExtensions.Hypermedia.Links;
using WebApi.HypermediaExtensions.WebApi.RouteResolver;
using WebApi.HypermediaExtensions.WebApi.Serializer.Reflection;
using Entity = WebApi.HypermediaExtensions.Hypermedia.Attributes.Entity;
using Link = WebApi.HypermediaExtensions.Hypermedia.Attributes.Link;
using Property = WebApi.HypermediaExtensions.Hypermedia.Attributes.Property;

namespace WebApi.HypermediaExtensions.WebApi.Serializer
{
    public static class ModelFactory2
    {
        public static Result<ImmutableDictionary<EntityKey, Bluehands.Hypermedia.Model.Entity>> Build(Assembly assembly, ModelBuilderOptions modelBuilderOptions) =>
            assembly
                .GetTypes()
                .Choose(t =>
                {
                    var hmoAttribute = t.GetCustomAttribute<HypermediaObjectAttribute>();
                    return hmoAttribute != null ? t : Option<Type>.None;
                })
                .Select(t => Build(t, modelBuilderOptions).Map(e => (key: t.ToEntityKey(), entity: e)))
                .Aggregate()
                .Map(entities => entities.ToImmutableDictionary(e => e.key, e => e.entity));

        public static Result<Bluehands.Hypermedia.Model.Entity> Build(Type hmoType, ModelBuilderOptions modelBuilderOptions)
        {
            return hmoType
                .TryGetAttribute<HypermediaObjectAttribute>()
                .Bind(bind: hmoAttribute => 
                {
                    var classes = hmoAttribute.Classes;
                    var title = hmoAttribute.Title;
                    var hmoTypeName = hmoType.Name;
                    var hmoTypeNamespace = hmoType.Namespace;

                    var propertyInfos = hmoType
                        .GetProperties()
                        .Where(p => !p.IsIgnored())
                        .ToImmutableArray();

                    return FindLinks(propertyInfos: propertyInfos)
                        .Map(links => modelBuilderOptions.CreateDefaultSelfLink ? EnsureSelfLink(links, hmoTypeName, hmoTypeNamespace) : links)
                        .Aggregate(
                            FindProperties(propertyInfos: propertyInfos),
                            FindEntities(propertyInfos))
                        .Map(t =>
                        {
                            var (links, propertyTuples, entities) = t;
                            
                            
                            var properties = propertyTuples.Select(tp => tp.property);
                            var keyProperties = propertyTuples.SelectMany(tp => tp.keyProperty);

                            return new Bluehands.Hypermedia.Model.Entity(
                                name: hmoTypeName,
                                title: title,
                                ns: hmoTypeNamespace,
                                classes: classes,
                                properties: properties,
                                keyProperties: keyProperties,
                                links: links,
                                entities: entities);
                        });

                })
                .Match(ok => ok,
                       error => Result.Error<Bluehands.Hypermedia.Model.Entity>($"Failed to created model for type {hmoType.Name}: {error}"));
        }

        static Result<List<Bluehands.Hypermedia.Model.Link>> FindLinks(ImmutableArray<PropertyInfo> propertyInfos)
        {
            return propertyInfos
                .GetAttributed<Link>()
                .Select(l =>
                {
                    var (linkAttribute, linkPropertyInfo) = l;
                    var propertyType = linkPropertyInfo.PropertyType;
                    var propertyName = linkPropertyInfo.Name;

                    Result<Bluehands.Hypermedia.Model.Link> Error(string message) =>
                        Result.Error<Bluehands.Hypermedia.Model.Link>(message);

                    Result<Bluehands.Hypermedia.Model.Link> WrongPropertyType() =>
                        Error($"Link property '{propertyName}' has unexpected type '{propertyType.Name}'. Supported link types are: {nameof(Uri)}, HypermediaObjectReference<T>, HypermediaObjectKeyReference<T>");

                    var relations = linkAttribute.Relations.ToImmutableArray();
                    if (propertyType == typeof(Uri))
                        return Bluehands.Hypermedia.Model.Link.ExternalReference(propertyName, relations);

                    if (!propertyType.IsConstructedGenericType)
                        return WrongPropertyType();

                    var genericType = propertyType.GetGenericTypeDefinition();
                    var isKeyReference = genericType == typeof(HypermediaObjectKeyReference<>);
                    var isObjReference = genericType == typeof(HypermediaObjectReference<>);

                    if (!isKeyReference && !isObjReference)
                        return WrongPropertyType();

                    var referencedType = propertyType.GetGenericArguments()[0];
                    if (!referencedType.IsHypermediaObject())
                        return Error($"Referenced type '{referencedType.Name}' in Link '{propertyName}' is no HypermediaObject type. HypermediaObject attribute missing?");

                    var entityKey = new EntityKey(referencedType.Name, referencedType.Namespace);

                    return isObjReference
                        ? Bluehands.Hypermedia.Model.Link.ObjectReference(propertyName, entityKey, relations)
                        : Bluehands.Hypermedia.Model.Link.KeyReference(propertyName, entityKey, relations);
                }).Aggregate();
        }

        private static List<Bluehands.Hypermedia.Model.Link> EnsureSelfLink(List<Bluehands.Hypermedia.Model.Link> links, string hmoTypeName, string hmoTypeNamespace)
        {
            if (links.Any(l => l.Relations.Select(r => r.ToLowerInvariant()).Contains(DefaultHypermediaRelations.Self)))
            {
                return links;
            }

            links.Add(new Bluehands.Hypermedia.Model.Link.KeyReference_("foo", new EntityKey(hmoTypeName, hmoTypeNamespace), new List<string>{ DefaultHypermediaRelations.Self }));
            return links;
        }

        static Result<List<SubEntity>> FindEntities(ImmutableArray<PropertyInfo> propertyInfos) =>
            propertyInfos
                .GetAttributed<Entity>()
                .Select(l =>
                {
                    var (entityAttribute, entityProperty) = l;
                    var propertyType = entityProperty.PropertyType;
                    var propertyName = entityProperty.Name;

                    Result<SubEntity> Error(string message) => Result.Error<SubEntity>(message);

                    Result<SubEntity> WrongPropertyType() =>
                        Error($"Sub entity property '{propertyName}' has unexpected type '{propertyType.Name}'. Supported sub entity types are: HypermediaObjectReference<T>, HypermediaObjectKeyReference<T>");

                    var relations = entityAttribute.Relations.ToImmutableArray();

                    if (!propertyType.IsConstructedGenericType)
                        return WrongPropertyType();

                    var genericType = propertyType.GetGenericTypeDefinition();
                    var isKeyReference = genericType == typeof(HypermediaObjectKeyReference<>);
                    var isObjReference = genericType == typeof(HypermediaObjectReference<>);

                    if (!isKeyReference && !isObjReference)
                        return WrongPropertyType();

                    var referencedType = propertyType.GetGenericArguments()[0];
                    if (!referencedType.IsHypermediaObject())
                        return Error($"Referenced type '{referencedType.Name}' in sub entity property '{propertyName}' is no HypermediaObject type. HypermediaObject attribute missing?");

                    var entityKey = new EntityKey(referencedType.Name, referencedType.Namespace);

                    return isObjReference
                        ? SubEntity.Embedded(propertyName, entityKey, relations)
                        : SubEntity.Link(propertyName, entityKey, relations);
                }).Aggregate();

        static Result<List<(Bluehands.Hypermedia.Model.Property property, Option<KeyProperty> keyProperty)>> FindProperties(ImmutableArray<PropertyInfo> propertyInfos) =>
            propertyInfos
                .GetHypermediaProperties()
                .Select(p =>
                {
                    Result<(Bluehands.Hypermedia.Model.Property, Option<KeyProperty>)> Error(string message) =>
                        Result.Error<(Bluehands.Hypermedia.Model.Property, Option<KeyProperty>)>(message);

                    var propertyType = p.GetType();
                    var propertyName = p.Name;

                    if (propertyType.IsHypermediaObject())
                        return Error($"Property '{propertyName}' can not be HypermediaObject itself. Please consider marking it with Link or Entity attribute to supply relations");

                    var propertyAttribute = p.GetCustomAttribute<Property>();

                    //TODO: further validations: deny IEnumerable<Hmo>, nested HMO properties, ...

                    var property = new Bluehands.Hypermedia.Model.Property(propertyName, propertyAttribute?.Name ?? propertyName, TypeDescriptor.CSharp(propertyType.Name, propertyType.FullName));
                    var keyAttribute = p.GetCustomAttribute<KeyAttribute>();
                    var keyProperty = keyAttribute != null
                        ? new KeyProperty(property, keyAttribute.TemplateParameterName)
                        : Option.None<KeyProperty>();

                    return (property, keyProperty);
                }).Aggregate();
    }


}