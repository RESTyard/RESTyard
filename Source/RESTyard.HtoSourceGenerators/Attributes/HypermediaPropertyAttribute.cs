using System;

namespace RESTyard.HtoSourceGenerators.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class HypermediaPropertyAttribute : Attribute, IHypermediaAttribute
{
    public string? Name { get; set; }
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class HypermediaLinkAttribute : Attribute, IHypermediaAttribute
{
    public string? Title { get; set; }
}