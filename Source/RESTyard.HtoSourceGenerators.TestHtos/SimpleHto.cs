using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Attributes;
using RESTyard.AspNetCore.Hypermedia.Links;

namespace RESTyard.HtoSourceGenerators.TestHtos;

/// <summary>
/// Minimal HTO: class, title, properties, self link.
/// Used as embedded entity target too.
/// </summary>
[HypermediaObject(Title = "A Customer", Classes = ["Customer"])]
public class SimpleCustomerHto : HypermediaObject
{
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
[HypermediaObject(Title = "An Address", Classes = ["Address"])]
public class AddressHto : HypermediaObject
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;

    [Relations(["self"])]
    public ILink<AddressHto> Self { get; set; }

    public AddressHto()
    {
        Self = Link.To(this);
    }
}
