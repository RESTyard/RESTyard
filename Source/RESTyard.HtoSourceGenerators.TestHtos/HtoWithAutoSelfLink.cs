using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Attributes;

namespace RESTyard.HtoSourceGenerators.TestHtos;

/// <summary>
/// HTO without an explicit self link — used to test AutoSelfLink behavior.
/// </summary>
[HypermediaObject(Title = "No Self Link", Classes = ["NoSelfLink"])]
public class HtoWithAutoSelfLink : HypermediaObject
{
    public string Name { get; set; } = string.Empty;
}
