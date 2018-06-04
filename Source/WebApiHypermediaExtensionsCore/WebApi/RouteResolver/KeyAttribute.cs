using System;

namespace WebApiHypermediaExtensionsCore.WebApi.RouteResolver
{
    public class KeyAttribute : Attribute
    {
        public string TemplateParameterName { get; set; }

        public KeyAttribute()
        {
        }

        public KeyAttribute(string templateParameterName)
        {
            TemplateParameterName = templateParameterName;
        }
    }
}