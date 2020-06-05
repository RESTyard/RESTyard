using System.Collections.Generic;
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
            CustomerRootReference = new HypermediaObjectReference<HypermediaCustomersRoot>(hypermediaCustomersRoot); 
            CarsRootReference = new HypermediaObjectReference<HypermediaCarsRoot>(hypermediaCarsRoot);
        }

        [Link(HypermediaLinks.EntryPoint.CustomersRoot)]
        public HypermediaObjectReference<HypermediaCustomersRoot> CustomerRootReference { get; set; }

        [Link(HypermediaLinks.EntryPoint.CarsRoot)]
        public HypermediaObjectReference<HypermediaCarsRoot> CarsRootReference { get; set; }

        //[Link("HypermediaObjectKeyReference")]
        //public HypermediaObjectReferenceBase HypermediaObjectKeyReference { get; set; } = new HypermediaObjectKeyReference<HypermediaCarsRoot>();

        //[Entity()]
        //[Link("HypermediaObjectKeyReference")]
        //public HypermediaObjectReferenceBase BaseReference { get; set; } = new HypermediaObjectReference<HypermediaCarsRoot>(new HypermediaCarsRoot());

    }
}
