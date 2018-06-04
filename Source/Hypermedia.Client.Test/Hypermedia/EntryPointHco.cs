namespace Hypermedia.Client.Test.Hypermedia
{
    using HypermediaClient.Hypermedia;
    using HypermediaClient.Hypermedia.Attributes;

    [HypermediaClientObject(Classes = new[] { "EntryPoint" })]
    public class EntryPointHco : HypermediaClientObject
    {
        [Mandatory]
        [HypermediaRelations(new [] {"self"})]
        public MandatoryHypermediaLink<EntryPointHco> Self { get; set; }

        [Mandatory]
        [HypermediaRelations(new[] { "CustomersRoot" })]
        public MandatoryHypermediaLink<CustomersRootHco> Customers { get; set; }
    }
}
