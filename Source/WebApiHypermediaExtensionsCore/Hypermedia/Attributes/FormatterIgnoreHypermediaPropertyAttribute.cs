using System;

namespace WebApiHypermediaExtensionsCore.Hypermedia.Attributes
{
    // default propperty formatting should not contain annotated properties
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FormatterIgnoreHypermediaPropertyAttribute : Attribute
    {
    }
}
