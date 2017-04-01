using System.Collections.Generic;
using HypermediaClient.Hypermedia;
using HypermediaClient.Hypermedia.Attributes;

namespace HypermediaClient.Test.Hypermedia
{
    [HypermediaClientObject(Classes = new[] { "CustomersQueryResult" })]
    public class CustomerQueryResultHco : HypermediaClientObject
    {
        [Mandatory]
        public int TotalEnties { get; set; }

        [Mandatory]
        public int CurrentEntitiesCount { get; set; }

        [HypermediaRelations(new[] { "Item" })] //TODO check: lists can not be mandatory, just empty
        public List<CustomerHco> Customers { get; set; }

        [Mandatory]
        //TODO: idear [EnsureUnique]? first hit will be used
        [HypermediaRelations( new[] { "Item" })]
        public CustomerHco Sister { get; set; }


        [Mandatory]
        [HypermediaRelations(new [] {"Self"})]
        public MandatoryHypermediaLink<CustomerQueryResultHco> Self { get; set; }

        [HypermediaRelations( new[] { "Next" })]
        public HypermediaLink<CustomerQueryResultHco> Next { get; set; }

        [HypermediaRelations( new[] { "Previous" })]
        public HypermediaLink<CustomerQueryResultHco> Previous { get; set; }

        [HypermediaRelations( new[] { "Last" })]
        public HypermediaLink<CustomerQueryResultHco> Last { get; set; }

        [HypermediaRelations( new[] { "All" })]
        public HypermediaLink<CustomerQueryResultHco> All { get; set; }

    }
}