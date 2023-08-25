using System;
using RESTyard.AspNetCore.JsonSchema;
using RESTyard.AspNetCore.WebApi.ExtensionMethods;

namespace RESTyard.AspNetCore.WebApi.RouteResolver
{
    /// <summary>
    /// Key value will be extracted automatically from Get route to HypermediaObject of type <see cref="ReferencedHypermediaObjectType"/>.
    /// Json schema for the parameter type (when created with <see cref="JsonSchemaFactory"/>) will expose an Uri property for every distinct <see cref="SchemaPropertyName"/>. 
    /// Use the <see cref="StartupExtensions.AddHypermediaParameterBinders"/> method to configure how binders are added for action parameter types using <see cref="KeyFromUriAttribute"/>  
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class KeyFromUriAttribute : Attribute
    {
        public string? SchemaPropertyName { get; }
        public string? RouteTemplateParameterName { get; }
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
        /// <param name="schemaPropertyName">Name of property in json schema</param>
        public KeyFromUriAttribute(Type referencedHypermediaObjectType, string schemaPropertyName) : this(referencedHypermediaObjectType)
        {
            SchemaPropertyName = schemaPropertyName;
        }

        /// <summary>
        /// Use for composite keys represented by multiple properties.
        /// </summary>
        /// <param name="schemaPropertyName">Name of property in json schema</param>
        /// <param name="routeTemplateParameterName">
        /// Has to match the template parameter name in GET route for type <see cref="ReferencedHypermediaObjectType"/> of the corresponding controller. 
        /// </param>
        /// <param name="referencedHypermediaObjectType"></param>
        public KeyFromUriAttribute(Type referencedHypermediaObjectType, string schemaPropertyName,
            string routeTemplateParameterName) : this(referencedHypermediaObjectType, schemaPropertyName)
        {
            RouteTemplateParameterName = routeTemplateParameterName;
        }
    }
}