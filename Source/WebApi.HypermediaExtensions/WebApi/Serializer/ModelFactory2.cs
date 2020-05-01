using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Bluehands.Hypermedia.Model;
using FunicularSwitch;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;
using WebApi.HypermediaExtensions.Hypermedia.Links;
using WebApi.HypermediaExtensions.WebApi.Serializer.Reflection;
using Entity = WebApi.HypermediaExtensions.Hypermedia.Attributes.Entity;
using Link = WebApi.HypermediaExtensions.Hypermedia.Attributes.Link;
using Property = Bluehands.Hypermedia.Model.Property;

namespace WebApi.HypermediaExtensions.WebApi.Serializer
{
    public static class ModelFactory2
    {
        public static Result<Bluehands.Hypermedia.Model.Entity> Build(Type hmoType)
        {
            return hmoType.TryGetAttribute<HypermediaObjectAttribute>()
                .Bind(bind: hmoAttribute =>
                {
                    var classes = hmoAttribute.Classes;
                    var title = hmoAttribute.Title;
                    var name = hmoType.Name;
                    var ns = hmoType.Namespace;

                    var propertyInfos = hmoType
                        .GetProperties()
                        .Where(p => !p.IsIgnored())
                        .ToImmutableArray();

                    return FindLinks(propertyInfos: propertyInfos).Aggregate(
                            FindProperties(propertyInfos: propertyInfos),
                            FindEntities(propertyInfos))
                        .Map(map: t =>
                        {
                            var (links, properties, entities) = t;

                            return new Bluehands.Hypermedia.Model.Entity(
                                name: name,
                                title: title,
                                ns: ns,
                                classes: classes,
                                properties: properties,
                                links: links,
                                entities);
                        });

                });
        }

        static Result<List<Bluehands.Hypermedia.Model.Link>> FindLinks(ImmutableArray<PropertyInfo> propertyInfos) =>
            propertyInfos
                .GetAttributed<Link>()
                .Select(l =>
                {
                    var (linkAttribute, linkPropertyInfo) = l;
                    var propertyType = linkPropertyInfo.PropertyType;
                    var propertyName = linkPropertyInfo.Name;

                    Result<Bluehands.Hypermedia.Model.Link> Error(string message) =>
                        Result.Error<Bluehands.Hypermedia.Model.Link>(message);

                    Result<Bluehands.Hypermedia.Model.Link> WrongPropertyType() =>
                        Error($"Unexpected link property type: {propertyType.Name}. Supported link types are: {nameof(Uri)}, HypermediaObjectReference<T>, HypermediaObjectKeyReference<T>");

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

        static Result<List<SubEntity>> FindEntities(ImmutableArray<PropertyInfo> propertyInfos) =>
            propertyInfos
                .GetAttributed<Entity>()
                .Select(l =>
                {
                    var (entityAttribute, entityProperty) = l;
                    var propertyType = entityProperty.GetType();
                    var propertyName = entityProperty.Name;

                    Result<SubEntity> Error(string message) =>
                        Result.Error<SubEntity>(message);

                    Result<SubEntity> WrongPropertyType() =>
                        Error($"Unexpected sub entity property type '{propertyType.Name}'. Supported sub entity types are: HypermediaObjectReference<T>, HypermediaObjectKeyReference<T>");

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

        static Result<List<Property>> FindProperties(ImmutableArray<PropertyInfo> propertyInfos) =>
            propertyInfos
                .GetHypermediaProperties()
                .Select(p =>
                {
                    Result<Property> Error(string message) =>
                        Result.Error<Property>(message);

                    var propertyType = p.GetType();
                    var propertyName = p.Name;

                    if (propertyType.IsHypermediaObject())
                        return Error($"Property '{propertyName}' can not be HypermediaObject itself. Please consider marking it with Link or Entity attribute to supply relations");

                    //TODO: further validations: deny IEnumerable<Hmo>, nested HMO properties, ...

                    return new Property(propertyName, TypeDescriptor.CSharp(propertyType.Name, propertyType.FullName));

                }).Aggregate();
    }


}