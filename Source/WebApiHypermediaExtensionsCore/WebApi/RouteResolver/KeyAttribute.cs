using System;

namespace WebApiHypermediaExtensionsCore.WebApi.RouteResolver
{
    /// <summary>
    /// Marks a property as (part of) the objects key. The route template to obtain the object 
    /// must contain parameters matching the TemplateParameterName property. If a single property contains the complete
    /// key information it is not neccessary and redundant to specify TemplateParameterName.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class KeyAttribute : Attribute
    {
        public string TemplateParameterName { get; }

        /// <summary>
        /// Use for simple keys represented by a single property.
        /// </summary>
        public KeyAttribute()
        {
        }

        /// <summary>
        /// Use for composite keys represented by multiple properties.
        /// </summary>
        /// <param name="templateParameterName">
        /// Has to match the template parameter name in GET route of the corresponding controller. 
        /// </param>
        public KeyAttribute(string templateParameterName)
        {
            TemplateParameterName = templateParameterName;
        }
    }
}