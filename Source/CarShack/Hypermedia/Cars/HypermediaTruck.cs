using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;

namespace CarShack.Hypermedia.Cars
{
    [HypermediaObject(Title = "A truck", Classes = new[] { "Truck" })]
    public class HypermediaTruck : HypermediaObject
    {
        public string Brand { get; set; }

        public int Id { get; set; }

        public HypermediaTruck(string brand, int id)
        {
            this.Brand = brand;
            this.Id = id;
        }
    }
}