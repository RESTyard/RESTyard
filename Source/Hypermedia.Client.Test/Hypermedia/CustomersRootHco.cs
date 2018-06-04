namespace Hypermedia.Client.Test.Hypermedia
{
    using global::Hypermedia.Relations;

    using HypermediaClient.Hypermedia;
    using HypermediaClient.Hypermedia.Attributes;
    using HypermediaClient.Hypermedia.Commands;

    [HypermediaClientObject(Classes = new[] { "CustomersRoot" })]
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