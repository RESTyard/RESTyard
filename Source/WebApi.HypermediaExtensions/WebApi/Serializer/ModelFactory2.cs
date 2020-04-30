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

                    var links = FindLinks(propertyInfos: propertyInfos);

                    return links.Map(map: l =>
                        new Bluehands.Hypermedia.Model.Entity(
                            name: name, 
                            title: title, 
                            ns: ns, 
                            classes: classes, 
                            //TODO:
                            properties: ImmutableArray<Property>.Empty,
                            links: l));

                });
        }

        static Result<List<Bluehands.Hypermedia.Model.Link>> FindLinks(ImmutableArray<PropertyInfo> propertyInfos) =>
            propertyInfos
                .GetAttributed<Link>()
                .Select(l =>
                {
                    var (linkAttribute, linkProperty) = l;
                    var propertyType = linkProperty.GetType();
                    var propertyName = linkProperty.Name;

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
                        return Error($"Referenced type in Link {propertyName} is no HypermediaObject type. HypermediaObject attribute missing?");

                    var entityKey = new EntityKey(referencedType.Name, referencedType.Namespace);

                    return isObjReference
                        ? Bluehands.Hypermedia.Model.Link.ObjectReference(propertyName, entityKey, relations)
                        : Bluehands.Hypermedia.Model.Link.KeyReference(propertyName, entityKey, relations);
                }).Aggregate();
    }
}