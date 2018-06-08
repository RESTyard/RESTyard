using System;

namespace WebApi.HypermediaExtensions.JsonSchema
{
    /// <summary>
    /// Key value will be automatically extracted from Get uri to Hypermediaobject of type <see cref="ReferencedHypermediaObjectType"/>.
    /// Json schema for the parameter type (when created with <see cref="JsonSchemaFactory"/>) will expose an Uri property for every distinct <see cref="SchemaProperyName"/>. 
    /// Use the <see cref="MvcOptionsExtensions.AddHypermediaParameterBinders"/> method to configure how binders are added for action parameter types using <see cref="KeyFromUriAttribute"/>  
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class KeyFromUriAttribute : Attribute
    {
        public string SchemaProperyName { get; }
        public string RouteTemplateParameterName { get; }
        public Type ReferencedHypermediaObjectType { get; }

        public KeyFromUriAttribute(Type referencedHypermediaObjectType)
        {
            ReferencedHypermediaObjectType = referencedHypermediaObjectType;
        }

        public KeyFromUriAttribute(Type referencedHypermediaObjectType, string schemaProperyName,
            string routeTemplateParameterName) : this(referencedHypermediaObjectType)
        {
            SchemaProperyName = schemaProperyName;
            RouteTemplateParameterName = routeTemplateParameterName;
        }
    }
}