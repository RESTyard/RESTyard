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

    public class RouteTemplateParameterAttribute : Attribute
    {
        public string ParameterName { get; set; }

        public RouteTemplateParameterAttribute()
        {
        }

        public RouteTemplateParameterAttribute(string parameterName)
        {
            ParameterName = parameterName;
        }
    }

    public class RouteKeyProducer : IKeyProducer
    {
        readonly IEnumerable<string> tempateParameterNames;

        //TODO: handle multiple template parameters
        public RouteKeyProducer(IEnumerable<string> tempateParameterNames)
        {
            this.tempateParameterNames = tempateParameterNames;
        }

        public object CreateFromHypermediaObject(HypermediaObject hypermediaObject)
        {
            //TODO: create accessor func and cache
            var property = hypermediaObject.GetType().GetProperties()
                .FirstOrDefault(p => p.GetCustomAttribute<RouteTemplateParameterAttribute>() != null);

            if (property != null)
            {
                return CreateFromKeyObject(property.GetValue(hypermediaObject));
            }

            throw new HypermediaException($"No property with key attribute found on type {hypermediaObject.GetType().Name}");
        }

        public object CreateFromKeyObject(object keyObject)
        {
            var dynamic = new ExpandoObject();
            var dict = (IDictionary<string, object>)dynamic;
            foreach (var name in tempateParameterNames)
            {
                dict.Add(name, keyObject);
            }
            return dynamic;
        }
    }

    public class RouteKeyProducer2 : IKeyProducer
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

        public static RouteKeyProducer2 Create(Type hypermediaObjectType, ICollection<string> templateParameterNames)
        {
            var keyProperties = hypermediaObjectType.GetProperties()
                .Select(p => new { p, att = p.GetCustomAttribute<RouteTemplateParameterAttribute>() })
                .Where(_ => _.att != null)
                .ToImmutableList();

            var paramsWithProperties = keyProperties.Select((k, i) => new
            {
                k.p,
                k.att,
                templateParameterName =
                    templateParameterNames.FirstOrDefault(n => i == 0 && k.att.ParameterName == null || n == k.att.ParameterName)
            }).ToImmutableList();

            var templateParametersWithoutAttributedProperties =
                templateParameterNames.Except(paramsWithProperties.Select(p => p.templateParameterName))
                .ToImmutableList();

            if (templateParametersWithoutAttributedProperties.Any())
            {
                throw new HypermediaException($"Route for type {hypermediaObjectType.Name} contains parameters '{string.Join(",", templateParametersWithoutAttributedProperties)}'. No property with attribute {nameof(RouteTemplateParameterAttribute)} was found on type {hypermediaObjectType.Name} for those properties.");
            }

            var propertiesWithoutTemplateParameter = paramsWithProperties.Where(p => p.templateParameterName == null).Select(p => p.p.Name).ToImmutableList();
            if (propertiesWithoutTemplateParameter.Any())
            {
                throw new HypermediaException($"Type {hypermediaObjectType.Name} contains properties with attribute {nameof(RouteTemplateParameterAttribute)} '{string.Join(",", propertiesWithoutTemplateParameter)}'. No template parameters found in route that correspond to those properties.");
            }

            var accessors = paramsWithProperties.Select(_ =>
                new Accessor(_.templateParameterName, MakeAccessor(hypermediaObjectType, _.p)));

            return new RouteKeyProducer2(accessors);
        }

        static Func<object, object> MakeAccessor(Type type, PropertyInfo propertyInfo)
        {
            var param = Expression.Parameter(typeof(object));
            var lambda = Expression.Lambda(Expression.Convert(Expression.Property(Expression.Convert(param, type), propertyInfo),
                typeof(object)));
            return (Func<object, object>) lambda.Compile();
        }

        //TODO: handle multiple template parameters
        public RouteKeyProducer2(IEnumerable<Accessor> keyAccessors)
        {
            this.keyAccessors = keyAccessors.ToImmutableList();
        }

        public object CreateFromHypermediaObject(HypermediaObject hypermediaObject)
        {
            //TODO: create accessor func and cache
            var property = hypermediaObject.GetType().GetProperties()
                .FirstOrDefault(p => p.GetCustomAttribute<RouteTemplateParameterAttribute>() != null);

            if (property != null)
            {
                return CreateFromKeyObject(property.GetValue(hypermediaObject));
            }

            throw new HypermediaException($"No property with key attribute found on type {hypermediaObject.GetType().Name}");
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

    /// <summary>
    /// Derive from this class to generate a KeyProducer for an <see cref="HypermediaObject"/>. 
    /// When building routes to HypermediaObjects the RouteResolver will try instanciate a RouteKeyProducer if provided in the route attributes and call CreateFromHypermediaObject.
    /// There can only be a constructor without parameters.
    /// </summary>
    public abstract class RouteKeyProducer<T, TKey> : IKeyProducer where T : HypermediaObject
    {
        public object CreateFromHypermediaObject(HypermediaObject hypermediaObject)
        {
            if (!(hypermediaObject is T typed))
            {
                throw new HypermediaException($"Passed object is not a {typeof(T)}");
            }

            return CreateKeyObject(GetKey(typed));
        }

        protected abstract TKey GetKey(T hypermediaObject);

        public object CreateFromKeyObject(object keyObject)
        {
            return CreateKeyObject((TKey)keyObject);
        }

        /// <summary>
        /// Must generate a anonymous object which is passed to an UrlHelper which generates a Route for a HypermediaObject. The <see cref="HypermediaObject"/> for which the route is build will be passed.
        /// The anonymous object must contain a field which is named exactly as the key in the route template so the UrlHelper matches.
        /// </summary>
        /// <param name="keyObject"></param>
        /// <returns></returns>
        protected abstract object CreateKeyObject(TKey keyObject);

    }
}