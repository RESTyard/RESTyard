using WebApiHypermediaExtensionsCore.Hypermedia;
using WebApiHypermediaExtensionsCore.Hypermedia.Attributes;
using WebApiHypermediaExtensionsCore.Hypermedia.Links;

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
        }
    }
}
