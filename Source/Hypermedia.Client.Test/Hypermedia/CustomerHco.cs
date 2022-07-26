using Bluehands.Hypermedia.Relations;
using RESTyard.Client.Hypermedia;
using RESTyard.Client.Hypermedia.Attributes;
using RESTyard.Client.Hypermedia.Commands;

namespace Bluehands.Hypermedia.Client.Test.Hypermedia
{
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

        //TODO maybe remove interfaces and make class HypermediaClientAction sealed
        [HypermediaCommand("MarkAsFavoriteAction")] // todo make name optional and use method name as default
        public IHypermediaClientAction<FavoriteCustomer> MarkAsFavorite { get; set; }

        [HypermediaCommand("CustomerMove")]
        public IHypermediaClientAction<NewAddress> CustomerMove { get; set; }
    }

    //TODO make a all generic HypermediaObjectGeneric to use in all cases, all fields are lists and dynamic

    //TODO not validated on read, maybe only on post
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
}


