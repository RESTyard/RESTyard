using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FunicularSwitch;
using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;
using WebApi.HypermediaExtensions.Util;
using Action = WebApi.HypermediaExtensions.Hypermedia.Attributes.Action;

namespace WebApi.HypermediaExtensions.WebApi.Serializer.Reflection
{
    public class ObjectReflectionService
    {
        public Result<ObjectReflection> Reflect(Type type)
        {
            var hypermediaObjectTypeResult = AssertTypeIsHypermediaObjectType(type);
            return BuildObjectReflection(
                hypermediaObjectTypeResult,
                GetReflectedProperties(hypermediaObjectTypeResult));
        }

        private Result<ObjectReflection> BuildObjectReflection(Result<Type> hypermediaObjectTypeResult, Result<List<ReflectedProperty>> reflectedPropertiesResult)
        {
            return AssertObjectAttributeIsPresent(hypermediaObjectTypeResult)
                .Aggregate(reflectedPropertiesResult, hypermediaObjectTypeResult)
                .Map(results =>
                {
                    var (hypermediaObjectAttribute, reflectedProperties, hypermediaObjectType) = results;
                    var links = GetLinks(reflectedProperties);
                    var properties = GetProperties(reflectedProperties);
                    var actions = GetActions(reflectedProperties);
                   var entities = GetEntities(reflectedProperties);
                    return new ObjectReflection(
                        hypermediaObjectType,
                        hypermediaObjectAttribute,
                        links,
                        properties,
                        actions,
                        entities
                    );
                });
        }

        private static List<ReflectedProperty> GetEntities(List<ReflectedProperty> reflectedProperties)
        {
            return reflectedProperties
                .Where(rp =>
                    rp.PrimaryHypermediaAttribute
                        .Map(a => a is Entity).Match(_ => _, () => false))
                .ToList();
        }

        private static List<ReflectedProperty> GetActions(List<ReflectedProperty> reflectedProperties)
        {
            return reflectedProperties
                .Where(rp =>
                    rp.PrimaryHypermediaAttribute
                        .Map(a => a is Action).Match(_ => _, () => false))
                .ToList();
        }

        private static List<ReflectedProperty> GetLinks(List<ReflectedProperty> reflectedProperties)
        {
            return reflectedProperties
                .Where(rp =>
                    rp.PrimaryHypermediaAttribute
                        .Map(a => a is Link).Match(_ => _, () => false))
                .ToList();
        }

        private static List<ReflectedProperty> GetProperties(List<ReflectedProperty> reflectedProperties)
        {
            return reflectedProperties
                .Where(rp =>
                    rp.PrimaryHypermediaAttribute.IsNone()
                    || rp.PrimaryHypermediaAttribute
                        .Map(a => a is Property).Match(_ => _, () => false))
                .ToList();
        }

        private Result<List<ReflectedProperty>> GetReflectedProperties(Result<Type> hypermediaObjectTypeResult)
        {
            return hypermediaObjectTypeResult.Bind(hypermediaObjectType =>
            {
                return hypermediaObjectType
                    .GetProperties()
                    .Select(propertyInfo =>
                        GetReflectedProperty(propertyInfo, hypermediaObjectType))
                    .Aggregate();
            });
        }

        private Result<ReflectedProperty> GetReflectedProperty(PropertyInfo propertyInfo, Type hypermediaObjectType)
        {
            try
            {
                var allAttributes = propertyInfo
                    .GetCustomAttributes()
                    .ToList();
                return AssertAttributesAreValidAndCreate(
                    propertyInfo,
                    hypermediaObjectType,
                    GetPrimaryHypermediaAttributes(allAttributes),
                    GetNonPrimaryHypermediaAttributes(allAttributes));
            }
            catch (Exception e)
            {
                return Result.Error<ReflectedProperty>($"In '{hypermediaObjectType.BeautifulName()}' at {propertyInfo.Name} the following error occured: {e}");
            }
        }

        private static List<Attribute> GetNonPrimaryHypermediaAttributes(IEnumerable<Attribute> allAttributes)
        {
            return allAttributes
                .Where(attribute =>
                    attribute is BaseHypermediaAttribute && !(attribute is Primary))
                .ToList();
        }

        private static List<Attribute> GetPrimaryHypermediaAttributes(IEnumerable<Attribute> attributes)
        {
            return attributes
                .Where(attribute =>
                    attribute is Primary)
                .ToList();
        }

        private Result<ReflectedProperty> AssertAttributesAreValidAndCreate(PropertyInfo propertyInfo, Type hypermediaObjectType, List<Attribute> primaryHypermediaAttributes,
            List<Attribute> nonPrimaryHypermediaAttributes)
        {
            
            return AssertOnlyOnePrimaryAttributeIsPresentAndCreate(
                    propertyInfo,
                    hypermediaObjectType,
                    primaryHypermediaAttributes,
                    nonPrimaryHypermediaAttributes)
                .Aggregate(AssertNoDuplicatesInNonPrimaryAttributes(
                    propertyInfo,
                    hypermediaObjectType,
                    nonPrimaryHypermediaAttributes))
                .Map(e => e.Item1);
        }

        private static Result<string> AssertNoDuplicatesInNonPrimaryAttributes(PropertyInfo propertyInfo, Type hypermediaObjectType, List<Attribute> nonPrimaryHypermediaAttributes)
        {
            var errors = nonPrimaryHypermediaAttributes
                .GroupBy(e => e.GetType())
                .Where(e => e.Count() > 1)
                .Select(e => e.Key)
                .Distinct()
                .Select(e => $"Duplicate attribute '{e.BeautifulName()}' at property {propertyInfo.Name} in class '{hypermediaObjectType.BeautifulName()}'")
                .ToArray();
            return errors.Any() ? Result.Error<string>(string.Join(Environment.NewLine, errors)) : Result.Ok("");
        }

        private static Result<ReflectedProperty> AssertOnlyOnePrimaryAttributeIsPresentAndCreate(PropertyInfo propertyInfo, Type hot,
            List<Attribute> primaryHypermediaAttributes, List<Attribute> nonPrimaryHypermediaAttributes)
        {
            if (primaryHypermediaAttributes.Count > 1)
            {
                return Result.Error<ReflectedProperty>(
                    $"Duplicate '{typeof(Primary).BeautifulName()}' attributes '{string.Join(", ", primaryHypermediaAttributes.Select(a => a.GetType().BeautifulName()))}' at property {propertyInfo.Name} in class '{hot.BeautifulName()}'");
            }
            var primaryHypermediaAttribute = primaryHypermediaAttributes.FirstOrDefault();
            return primaryHypermediaAttribute != null
                ? new ReflectedProperty(propertyInfo, primaryHypermediaAttribute, nonPrimaryHypermediaAttributes)
                : new ReflectedProperty(propertyInfo, nonPrimaryHypermediaAttributes);
        }

        private static Result<Type> AssertTypeIsHypermediaObjectType(Type hypermediaObjectType)
        {
            return typeof(HypermediaObject).IsAssignableFrom(hypermediaObjectType)
                ? Result<Type>.Ok(hypermediaObjectType)
                : Result.Error<Type>($"The Type '{hypermediaObjectType.BeautifulName()}' is not assignable to a {typeof(HypermediaObject).BeautifulName()}.");
        }

        public Result<HypermediaObjectAttribute> AssertObjectAttributeIsPresent(Result<Type> hypermediaObjectTypeResult)
        {
            return hypermediaObjectTypeResult.Bind(hypermediaObjectType =>
            {
                try
                {
                    var objectAttribute =
                        hypermediaObjectType.GetTypeInfo()
                            .GetCustomAttribute<HypermediaObjectAttribute>();
                    return objectAttribute is null
                        ? Result.Error<HypermediaObjectAttribute>
                            ($"Missing '{GetObjectAttributeBeautifulName()}' in '{hypermediaObjectType.BeautifulName()}'. As it is required add it to your class")
                        : Result.Ok(objectAttribute);
                }
                catch (Exception e)
                {
                    return Result.Error<HypermediaObjectAttribute>
                        ($"Get {GetObjectAttributeBeautifulName()} failed in '{hypermediaObjectType.BeautifulName()}' with Exception: {e}.");
                }
            });
        }
        private static string GetObjectAttributeBeautifulName()
        {
            return typeof(HypermediaObjectAttribute).BeautifulName();
        }
    }
}