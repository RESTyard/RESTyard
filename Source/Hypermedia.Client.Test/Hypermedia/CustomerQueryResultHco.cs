namespace Hypermedia.Client.Test.Hypermedia
{
    using System.Collections.Generic;

    using HypermediaClient.Hypermedia;
    using HypermediaClient.Hypermedia.Attributes;

    [HypermediaClientObject(Classes = new[] { "CustomersQueryResult" })]
    public class CustomerQueryResultHco : HypermediaClientObject
    {
        [Mandatory]
        public int TotalEnties { get; set; }

        [Mandatory]
        public int CurrentEntitiesCount { get; set; }

        [HypermediaRelations(new[] { "item" })] //TODO check: lists can not be mandatory, just empty
        public List<CustomerHco> Customers { get; set; }

        [Mandatory]
        //TODO: idear [EnsureUnique]? first hit will be used
        [HypermediaRelations( new[] { "item" })]
        public CustomerHco Sister { get; set; }


        [Mandatory]
        [HypermediaRelations(new [] {"self"})]
        public MandatoryHypermediaLink<CustomerQueryResultHco> Self { get; set; }

        [HypermediaRelations( new[] { "next" })]
        public HypermediaLink<CustomerQueryResultHco> Next { get; set; }

        [HypermediaRelations( new[] { "previous" })]
        public HypermediaLink<CustomerQueryResultHco> Previous { get; set; }

        [HypermediaRelations( new[] { "last" })]
        public HypermediaLink<CustomerQueryResultHco> Last { get; set; }

        [HypermediaRelations( new[] { "all" })]
        public HypermediaLink<CustomerQueryResultHco> All { get; set; }

    }
}