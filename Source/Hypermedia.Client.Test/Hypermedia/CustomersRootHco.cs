using System;
using Bluehands.Hypermedia.Relations;
using RESTyard.Client.Hypermedia;
using RESTyard.Client.Hypermedia.Attributes;
using RESTyard.Client.Hypermedia.Commands;

namespace RESTyard.Client.Test.Hypermedia
{
    [HypermediaClientObject("CustomersRoot")]
    public class CustomersRootHco : HypermediaClientObject
    {
        [Mandatory]
        [HypermediaRelations(new[] { DefaultHypermediaRelations.Self })]
        public MandatoryHypermediaLink<CustomersRootHco> Self { get; set; }

        [Mandatory]
        [HypermediaRelations(new[] { DefaultHypermediaRelations.Queries.All })]
        public MandatoryHypermediaLink<CustomerQueryResultHco> All { get; set; }

        [Mandatory]
        [HypermediaCommand("CreateQuery")]
        public IHypermediaClientFunction<CustomerQueryResultHco, CustomersQuery> CreateQuery { get; set; }
    }

    public class CustomersQuery
    {
        public CustomerFilter Filter { get; set; }
        public SortOptions SortBy { get; set; }
        public Pagination Pagination { get; set; }
    }

    public class Pagination
    {
        public int PageSize { get; set; }
        public int PageOffset { get; set; }
    }

    public class SortOptions
    {
        public string PropertyName { get; set; }
        public string SortType { get; set; }
    }

    public class CustomerFilter
    {
        public int MinAge { get; set; }
    }
}