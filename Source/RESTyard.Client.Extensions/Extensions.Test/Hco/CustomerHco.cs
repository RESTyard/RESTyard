using RESTyard.Client.Hypermedia;
using RESTyard.Client.Hypermedia.Attributes;
using RESTyard.Client.Hypermedia.Commands;
using RESTyard.Relations;

namespace Extensions.Test.Hco;

[HypermediaClientObject("Customer")]
public class CustomerHco : HypermediaClientObject
{
    [Mandatory]
    public string FullName { get; set; }

    public int Age { get; set; }

    public string Address { get; set; }

    public bool IsFavorite { get; set; }

    [Mandatory]
    [HypermediaRelations(new[] { DefaultHypermediaRelations.Self })]
    public MandatoryHypermediaLink<CustomerHco> Self { get; set; }

    [HypermediaCommand("MarkAsFavoriteAction")]
    public IHypermediaClientAction<FavoriteCustomer> MarkAsFavorite { get; set; }

    [HypermediaCommand("CustomerMove")]
    public IHypermediaClientAction<NewAddress> CustomerMove { get; set; }
}

[HypermediaCommandParameter(new[] { "http://localhost:5000/Customers/NewAddressType" })]
public class NewAddress
{
    public string Address { get; set; }
}

[HypermediaCommandParameter(new[] { "http://localhost:5000/MyFavoriteCustomers/FavoriteCustomer" })]
public class FavoriteCustomer
{
    public string Customer { get; set; }
}