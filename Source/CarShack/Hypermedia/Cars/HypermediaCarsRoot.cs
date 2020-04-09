using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;
using WebApi.HypermediaExtensions.Hypermedia.Links;

namespace CarShack.Hypermedia.Cars
{
    [HypermediaObject(Title = "The Cars API", Classes = new[] { "CarsRoot" })]
    public class HypermediaCarsRoot : HypermediaObject
    {
        public HypermediaCarsRoot()
        {
            // added as instance
            NiceCarReference = new HypermediaObjectReference<HypermediaCar>(new HypermediaCar("VW", 2));

            // added as type reference with key object
            SuperCarReference = new HypermediaObjectKeyReference<HypermediaCar>(new { brand = "Porsche", key = 5 });

            // Add object with no corresponding route. This will throw an exception if serialized
            // or a default route if HypermediaExtensionsOptions.ReturnDefaultRouteForUnknownHto is set to true
            // Links.Add("Truck", new SelfQueryReference(new HypermediaTruck("Daimler", 11)));
        }

        [Link("NiceCar")]
        public HypermediaObjectReference<HypermediaCar> NiceCarReference { get; set; }

        [Link("SuperCar")]
        public HypermediaObjectKeyReference<HypermediaCar> SuperCarReference { get; set; }
    }
}
