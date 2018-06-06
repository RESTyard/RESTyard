using System;

namespace WebApi.HypermediaExtensions.Test.JsonSchema
{
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