using CarShack.Hypermedia.Cars;
using CarShack.Hypermedia.Customers;
using WebApi.HypermediaExtensions.Hypermedia.Links;
using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;

namespace CarShack.Hypermedia.EntryPoint
{
    [HypermediaObject(Title = "Entry to the Rest API", Classes = new[] { "EntryPoint" })]
    public class HypermediaEntryPoint : HypermediaObject
    {
        public HypermediaEntryPoint(HypermediaCustomersRoot hypermediaCustomersRoot, HypermediaCarsRoot hypermediaCarsRoot)
        {
            Links.Add(HypermediaLinks.EntryPoint.CustomersRoot, new HypermediaObjectReference(hypermediaCustomersRoot));     
            Links.Add(HypermediaLinks.EntryPoint.CarsRoot, new HypermediaObjectReference(hypermediaCarsRoot));
        }
    }
}
