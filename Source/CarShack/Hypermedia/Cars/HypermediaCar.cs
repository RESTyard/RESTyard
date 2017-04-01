using WebApiHypermediaExtensionsCore.Hypermedia;
using WebApiHypermediaExtensionsCore.Hypermedia.Attributes;

namespace CarShack.Hypermedia.Cars
{
    [HypermediaObject(Title = "A Car", Classes = new[] { "Car" })]
    public class HypermediaCar : HypermediaObject
    {
        public string Brand { get; set; }

        public int Id { get; set; }

        public HypermediaCar(string brand, int id)
        {
            this.Brand = brand;
            this.Id = id;
        }
    }
}