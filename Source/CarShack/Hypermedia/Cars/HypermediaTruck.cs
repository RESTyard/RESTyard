using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Attributes;

namespace CarShack.Hypermedia.Cars
{
    // this HTO has no route. Exists to demonstrate the use of ReturnDefaultRouteForUnknownHto.
    [HypermediaObject(Title = "A truck", Classes = new[] { "Truck" })]
    public class HypermediaTruck : IHypermediaObject
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