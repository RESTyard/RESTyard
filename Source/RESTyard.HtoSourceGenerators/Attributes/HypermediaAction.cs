using System;

namespace RESTyard.HtoSourceGenerators.Attributes;

public class HypermediaActionAttribute : Attribute
{
    public string Name { get; set; }
    public string Title { get; set; }
}