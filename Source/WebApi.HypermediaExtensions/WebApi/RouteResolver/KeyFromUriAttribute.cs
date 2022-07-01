using System;
using WebApi.HypermediaExtensions.JsonSchema;
using WebApi.HypermediaExtensions.WebApi.ExtensionMethods;

namespace WebApi.HypermediaExtensions.WebApi.RouteResolver
{
    /// <summary>
    /// Key value will be  extracted automatically from Get route to Hypermediaobject of type <see cref="ReferencedHypermediaObjectType"/>.
    /// Json schema for the parameter type (when created with <see cref="JsonSchemaFactory"/>) will expose an Uri property for every distinct <see cref="SchemaProperyName"/>. 
    /// Use the <see cref="StartupExtensions.AddHypermediaParameterBinders"/> method to configure how binders are added for action parameter types using <see cref="KeyFromUriAttribute"/>  
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class KeyFromUriAttribute : Attribute
    {
        public string SchemaProperyName { get; }
        public string RouteTemplateParameterName { get; }
        public Type ReferencedHypermediaObjectType { get; }

        /// <summary>
        /// Use when a single property represents the objects key. Property name in json schema will be the attributed properties name. 
        /// </summary>
        /// <param name="referencedHypermediaObjectType"></param>
        public KeyFromUriAttribute(Type referencedHypermediaObjectType)
        {
            ReferencedHypermediaObjectType = referencedHypermediaObjectType;
        }

        /// <summary>
        /// Use when a single property represents the objects key /// </summary>
        /// <param name="referencedHypermediaObjectType"></param>
        /// <param name="schemaProperyName">Name of property in json schema</param>
        public KeyFromUriAttribute(Type referencedHypermediaObjectType, string schemaProperyName) : this(referencedHypermediaObjectType)
        {
            SchemaProperyName = schemaProperyName;
        }

        /// <summary>
        /// Use for composite keys represented by multiple properties.
        /// </summary>
        /// <param name="schemaProperyName">Name of property in json schema</param>
        /// <param name="routeTemplateParameterName">
        /// Has to match the template parameter name in GET route for type <see cref="ReferencedHypermediaObjectType"/> of the corresponding controller. 
        /// </param>
        /// <param name="referencedHypermediaObjectType"></param>
        public KeyFromUriAttribute(Type referencedHypermediaObjectType, string schemaProperyName,
            string routeTemplateParameterName) : this(referencedHypermediaObjectType, schemaProperyName)
        {
            RouteTemplateParameterName = routeTemplateParameterName;
        }
    }
}