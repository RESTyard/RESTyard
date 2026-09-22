using Json.Schema.Generation;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Attributes;
using RESTyard.AspNetCore.Hypermedia.Links;

namespace RESTyard.HtoSourceGenerators.TestHtos;

/// <summary>
/// Minimal HTO: class, title, properties, self link.
/// Used as embedded entity target too.
/// </summary>
[Title("A Customer")]
[HypermediaObject(Classes = ["Customer"])]
public class SimpleCustomerHto : IHypermediaObject
{
    public string? HtoTitle => "A Customer";

    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }

    [Relations(["self"])]
    public ILink<SimpleCustomerHto> Self { get; set; }

    public SimpleCustomerHto()
    {
        Self = Link.To(this);
    }
}

/// <summary>
/// Tiny HTO for nested embedded entity testing.
/// </summary>
[Title("An Address")]
[HypermediaObject(Classes = ["Address"])]
public class AddressHto : IHypermediaObject
{
    public string? HtoTitle => "An Address";

    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;

    [Relations(["self"])]
    public ILink<AddressHto> Self { get; set; }

    public AddressHto()
    {
        Self = Link.To(this);
    }
}
