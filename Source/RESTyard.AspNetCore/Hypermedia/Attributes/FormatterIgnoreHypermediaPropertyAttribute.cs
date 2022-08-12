using System;

namespace RESTyard.AspNetCore.Hypermedia.Attributes
{
    // default propperty formatting should not contain annotated properties
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FormatterIgnoreHypermediaPropertyAttribute : Attribute
    {
    }
}
