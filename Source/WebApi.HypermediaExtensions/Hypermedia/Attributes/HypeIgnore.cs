using System;

namespace WebApi.HypermediaExtensions.Hypermedia.Attributes
{
    // default propperty formatting should not contain annotated properties
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class HypeIgnore : HypePrimary
    {
    }
}
