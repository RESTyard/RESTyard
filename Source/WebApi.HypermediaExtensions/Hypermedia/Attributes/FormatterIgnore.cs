using System;

namespace WebApi.HypermediaExtensions.Hypermedia.Attributes
{
    // default property formatting should not contain annotated properties
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FormatterIgnore : Primary
    {
    }
}
