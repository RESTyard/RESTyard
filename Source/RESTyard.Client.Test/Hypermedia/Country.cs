#nullable enable
using System.Xml.Linq;

namespace RESTyard.Client.Test.Hypermedia;

public class Country
{
    public string Name { get; set; } = string.Empty;
    
    public int Population { get; set; }
}

public class CustomerQuery
{

}

public class MarkAsFavoriteParameters
{

}