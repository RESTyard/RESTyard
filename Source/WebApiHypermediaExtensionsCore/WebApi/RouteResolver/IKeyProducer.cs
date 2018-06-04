using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using WebApiHypermediaExtensionsCore.Exceptions;
using WebApiHypermediaExtensionsCore.Hypermedia;
using WebApiHypermediaExtensionsCore.Hypermedia.Links;

namespace WebApiHypermediaExtensionsCore.WebApi.RouteResolver
{
    /// <summary>
    /// Derive from this interface to generate a KeyProducer for an <see cref="HypermediaObject"/>. 
    /// When building routes to HypermediaObjects the RouteResolver will try instanciate a RouteKeyProducer if provided in the route attributes and call CreateFromHypermediaObject.
    /// There can only be a constructor without parameters.
    /// </summary>
    public interface IKeyProducer
    {
        /// <summary>
        /// Must generate a anonymous object which is passed to an UrlHelper which generates a Route for a HypermediaObject. The <see cref="HypermediaObject"/> for which the route is build will be passed.
        /// The anonymous object must contain a field which is named exactly as the key in the route template so the UrlHelper matches.
        /// </summary>
        /// <param name="hypermediaObject"></param>
        /// <returns></returns>
        object CreateFromHypermediaObject(HypermediaObject hypermediaObject);

        /// <summary>
        /// Must generate a anonymous object which is passed to an UrlHelper which generates a Route for a HypermediaObject.
        /// This method is called if a <see cref="HypermediaObjectKeyReference"/> must be resolved to a route (and therefor no instance is available).
        /// The anonymous object must contain a field which is named exactly as the key in the route template so the UrlHelper matches.
        /// </summary>
        /// <param name="keyObject">The key passed to <see cref="HypermediaObjectKeyReference"/>.</param>
        /// <returns></returns>
        object CreateFromKeyObject(object keyObject);
    }

    public class Key : Attribute
    {
        public string TemplateParameterName { get; set; }

        public Key()
        {
        }

        public Key(string templateParameterName)
        {
            TemplateParameterName = templateParameterName;
        }
    }

    public class RouteKeyProducer : IKeyProducer
    {
        readonly ImmutableList<Accessor> keyAccessors;

        public class Accessor
        {
            public string TemplateParameterName { get; }

            public Func<object, object> GetKey { get; }

            public Accessor(string templateParameterName, Func<object, object> getKey)
            {
                TemplateParameterName = templateParameterName;
                GetKey = getKey;
            }
        }

        public static RouteKeyProducer Create(Type hypermediaObjectType, ICollection<string> templateParameterNames)
        {
            var keyProperties = hypermediaObjectType.GetProperties()
                .Select(p => new { p, att = p.GetCustomAttribute<Key>() })
                .Where(_ => _.att != null)
                .ToImmutableList();

            var paramsWithProperties = keyProperties.Select((k, i) => new
            {
                k.p,
                k.att,
                templateParameterName =
                    templateParameterNames.FirstOrDefault(n => i == 0 && k.att.TemplateParameterName == null || n == k.att.TemplateParameterName)
            }).ToImmutableList();

            var templateParametersWithoutAttributedProperties =
                templateParameterNames.Except(paramsWithProperties.Select(p => p.templateParameterName))
                .ToImmutableList();

            if (templateParametersWithoutAttributedProperties.Any())
            {
                throw new HypermediaException($"Route for type {hypermediaObjectType.Name} contains parameters '{string.Join(",", templateParametersWithoutAttributedProperties)}'. No property with attribute {nameof(Key)} was found on type {hypermediaObjectType.Name} for those properties.");
            }

            var propertiesWithoutTemplateParameter = paramsWithProperties.Where(p => p.templateParameterName == null).Select(p => p.p.Name).ToImmutableList();
            if (propertiesWithoutTemplateParameter.Any())
            {
                throw new HypermediaException($"Type {hypermediaObjectType.Name} contains properties with attribute {nameof(Key)} '{string.Join(",", propertiesWithoutTemplateParameter)}'. No template parameters found in route that correspond to those properties.");
            }

            var accessors = paramsWithProperties.Select(_ =>
                new Accessor(_.templateParameterName, MakeAccessor(hypermediaObjectType, _.p)));

            return new RouteKeyProducer(accessors);
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

        public RouteKeyProducer(IEnumerable<Accessor> keyAccessors)
        {
            this.keyAccessors = keyAccessors.ToImmutableList();
        }

        public object CreateFromHypermediaObject(HypermediaObject hypermediaObject)
        {
            var dynamic = new ExpandoObject();
            var dict = (IDictionary<string, object>)dynamic;
            foreach (var accessor in keyAccessors)
            {
                dict.Add(accessor.TemplateParameterName, accessor.GetKey(hypermediaObject));
            }
            return dynamic;
        }

        public object CreateFromKeyObject(object keyObject)
        {
            var dynamic = new ExpandoObject();
            var dict = (IDictionary<string, object>)dynamic;
            foreach (var accessor in keyAccessors)
            {
                dict.Add(accessor.TemplateParameterName, accessor.GetKey(keyObject));
            }
            return dynamic;
        }
    }
}