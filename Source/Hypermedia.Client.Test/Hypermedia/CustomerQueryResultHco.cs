using System.Collections.Generic;
using Bluehands.Hypermedia.Client.Hypermedia;
using Bluehands.Hypermedia.Client.Hypermedia.Attributes;
using Bluehands.Hypermedia.Relations;

namespace Bluehands.Hypermedia.Client.Test.Hypermedia
{
    [HypermediaClientObject("CustomersQueryResult")]
    public class CustomerQueryResultHco : HypermediaClientObject
    {
        [Mandatory]
        public int TotalEntities { get; set; }

        [Mandatory]
        public int CurrentEntitiesCount { get; set; }

        [HypermediaRelations(new[] { DefaultHypermediaRelations.EmbeddedEntities.Item })] //TODO check: lists can not be mandatory, just empty
        public List<CustomerHco> Customers { get; set; }

        [Mandatory]
        //TODO: idear [EnsureUnique]? first hit will be used
        [HypermediaRelations(new[] { DefaultHypermediaRelations.EmbeddedEntities.Item })]
        public CustomerHco Sister { get; set; }


        [Mandatory]
        [HypermediaRelations(new [] { DefaultHypermediaRelations.Self })]
        public MandatoryHypermediaLink<CustomerQueryResultHco> Self { get; set; }

        [HypermediaRelations( new[] { DefaultHypermediaRelations.Queries.Next })]
        public HypermediaLink<CustomerQueryResultHco> Next { get; set; }

        [HypermediaRelations( new[] { DefaultHypermediaRelations.Queries.Previous })]
        public HypermediaLink<CustomerQueryResultHco> Previous { get; set; }

        [HypermediaRelations( new[] { DefaultHypermediaRelations.Queries.Last })]
        public HypermediaLink<CustomerQueryResultHco> Last { get; set; }

        [HypermediaRelations( new[] { DefaultHypermediaRelations.Queries.All })]
        public HypermediaLink<CustomerQueryResultHco> All { get; set; }

    }
}