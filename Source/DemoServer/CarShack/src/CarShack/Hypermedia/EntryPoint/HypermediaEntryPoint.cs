using CarShack.Hypermedia.Customers;
using WebApiHypermediaExtensionsCore.Hypermedia.Links;

namespace CarShack.Hypermedia.EntryPoint
{
    
    using WebApiHypermediaExtensionsCore.Hypermedia;
    using WebApiHypermediaExtensionsCore.Hypermedia.Attributes;

    [HypermediaObject(Title = "Entry to the Rest API", Classes = new[] { "EntryPoint" })]
    public class HypermediaEntryPoint : HypermediaObject
    {
        public HypermediaEntryPoint(HypermediaCustomersRoot hypermediaCustomersRoot)
        {
            Links.Add(HypermediaLinks.EntryPoint.CustomersRoot, new HypermediaObjectReference(hypermediaCustomersRoot));     
        }
    }
}
