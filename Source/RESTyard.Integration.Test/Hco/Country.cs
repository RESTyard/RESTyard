#nullable enable
using RESTyard.Client.Hypermedia.Commands;
using System.Collections.Generic;
using System.IO;
using System;
using System.Xml.Linq;

namespace RESTyard.Integration.Test.Hco;

public class Country
{
    public string Name { get; set; } = string.Empty;
    
    public int Population { get; set; }
}

public record MarkAsFavoriteParameters(string Customer)
{
    public static MarkAsFavoriteParameters FromCustomer(HypermediaCustomerHco c) => new(c.Self.Uri!.ToString());
}