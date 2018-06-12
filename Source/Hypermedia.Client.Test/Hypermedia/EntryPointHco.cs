using Hypermedia.Relations;

namespace Hypermedia.Client.Test.Hypermedia
{
    using global::Hypermedia.Client.Hypermedia;
    using global::Hypermedia.Client.Hypermedia.Attributes;

    [global::Hypermedia.Client.Hypermedia.Attributes.HypermediaClientObjectAttribute(Classes = new[] { "EntryPoint" })]
    public class EntryPointHco : HypermediaClientObject
    {
        [Mandatory]
        [HypermediaRelations(new [] { DefaultHypermediaRelations.Self })]
        public MandatoryHypermediaLink<EntryPointHco> Self { get; set; }

        [Mandatory]
        [HypermediaRelations(new[] { "CustomersRoot" })]
        public MandatoryHypermediaLink<CustomersRootHco> Customers { get; set; }
    }
}
