using Json.Schema.Generation;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Attributes;

namespace RESTyard.HtoSourceGenerators.TestHtos;

/// <summary>
/// HTO without an explicit self link — used to test AutoSelfLink behavior.
/// </summary>
[Title("No Self Link")]
[HypermediaObject(Classes = ["NoSelfLink"])]
public class HtoWithAutoSelfLink : IHypermediaObject
{
    public string? HtoTitle => "No Self Link";

    public string Name { get; set; } = string.Empty;
}
