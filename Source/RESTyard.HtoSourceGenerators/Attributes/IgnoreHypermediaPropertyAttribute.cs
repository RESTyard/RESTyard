using System;

namespace RESTyard.HtoSourceGenerators.Attributes
{
    // default property formatting should not contain annotated properties
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class IgnoreHypermediaPropertyAttribute : Attribute, IHypermediaAttribute
    {
    }
}
