using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using RESTyard.AspNetCore.Exceptions;
using RESTyard.AspNetCore.Hypermedia;

namespace RESTyard.AspNetCore.WebApi.RouteResolver
{
    public class RouteKeyProducer : IKeyProducer
    {
        readonly bool isComplexKey;
        readonly ImmutableList<Accessor> keyAccessors;

        public static RouteKeyProducer Create(Type hypermediaObjectType, ICollection<string?> templateParameterNames)
        {
            var accessors = MakeAccessors(hypermediaObjectType, templateParameterNames);
            return new RouteKeyProducer(accessors);
        }

        static IEnumerable<Accessor> MakeAccessors(Type hypermediaObjectType, ICollection<string?> templateParameterNames)
        {
            var keyProperties = hypermediaObjectType.GetTypeInfo().GetProperties()
                .Select(p => (p, att: p.GetCustomAttribute<KeyAttribute>()))
                .Where(_ => _.att is not null)
                .Select(tuple => (tuple.p, att: tuple.att!))
                .ToImmutableList();

            var paramsWithProperties = keyProperties.Select((k, i) => (
                k.p,
                k.att,
                templateParameterName: 
                    templateParameterNames.FirstOrDefault(
                        n => i == 0 && k.att.TemplateParameterName == null || n == k.att.TemplateParameterName)
            )).ToImmutableList();

            var templateParametersWithoutAttributedProperties =
                templateParameterNames.Except(paramsWithProperties.Select(p => p.templateParameterName))
                    .ToImmutableList();

            if (templateParametersWithoutAttributedProperties.Any())
            {
                throw new HypermediaException(
                    $"Route for type {hypermediaObjectType.Name} contains parameters '{string.Join(",", templateParametersWithoutAttributedProperties)}'. No property with attribute {nameof(KeyAttribute)} was found on type {hypermediaObjectType.Name} for those properties.");
            }

            var propertiesWithoutTemplateParameter = paramsWithProperties.Where(p => p.templateParameterName == null)
                .Select(p => p.p.Name).ToImmutableList();
            if (propertiesWithoutTemplateParameter.Any())
            {
                throw new HypermediaException(
                    $"Type {hypermediaObjectType.Name} contains properties with attribute {nameof(KeyAttribute)} '{string.Join(",", propertiesWithoutTemplateParameter)}'. No template parameters found in route that correspond to those properties.");
            }

            var accessors = paramsWithProperties.Select(tuple =>
                new Accessor(tuple.templateParameterName!, MakeAccessor(hypermediaObjectType, tuple.p)));
            return accessors;
        }

        static Func<object, object> MakeAccessor(Type type, PropertyInfo propertyInfo)
        {
            var param = Expression.Parameter(typeof(object));
            var lambda = Expression.Lambda(
                Expression.Convert(
                    Expression.Property(Expression.Convert(param, type), propertyInfo),
                    typeof(object)), param);
            return (Func<object, object>) lambda.Compile();
        }

        private RouteKeyProducer(IEnumerable<Accessor> keyAccessors)
        {
            this.keyAccessors = keyAccessors.ToImmutableList();
            isComplexKey = this.keyAccessors.Count > 1;
        }

        public object CreateFromHypermediaObject(HypermediaObject hypermediaObject)
        {
            var dynamic = new ExpandoObject();
            var dict = (IDictionary<string, object?>)dynamic;
            foreach (var accessor in keyAccessors)
            {
                dict.Add(accessor.TemplateParameterName, accessor.GetKey(hypermediaObject));
            }
            return dynamic;
        }

        public object? CreateFromKeyObject(object? keyObject)
        {
            if (isComplexKey)
            {
                return keyObject;
            }

            var dynamic = new ExpandoObject();
            var dict = (IDictionary<string, object?>)dynamic;
            foreach (var accessor in keyAccessors)
            {
                dict.Add(accessor.TemplateParameterName, keyObject);
            }
            return dynamic;
        }

        class Accessor
        {
            public string TemplateParameterName { get; }

            public Func<object, object> GetKey { get; }

            public Accessor(string templateParameterName, Func<object, object> getKey)
            {
                TemplateParameterName = templateParameterName;
                GetKey = getKey;
            }
        }
    }
}