using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
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
    }

    public class RouteKeyProducer : IKeyProducer
    {
        readonly string m_KeyTemplateName;

        //TODO: handle multiple template parameters
        public RouteKeyProducer(string keyTemplateName)
        {
            m_KeyTemplateName = keyTemplateName;
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
            ((IDictionary<string, object>)dynamic).Add(m_KeyTemplateName, keyObject);
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