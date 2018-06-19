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
            Links.Add("NiceCar", new HypermediaObjectReference(new HypermediaCar("VW", 2)));

            // added as type reference with key object
            Links.Add("SuperCar", new HypermediaObjectKeyReference(typeof(HypermediaCar), new { brand = "Porsche", key = 5 }));

            // Add object with no corresponding route. This will throw an exception if serialized
            // or a default route if HypermediaExtensionsOptions.ReturnDefaultRouteForUnknownHto is set to true
            // Links.Add("Truck", new HypermediaObjectReference(new HypermediaTruck("Daimler", 11)));
        }
    }
}
