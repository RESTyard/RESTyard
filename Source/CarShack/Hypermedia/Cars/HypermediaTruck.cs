using Json.Schema.Generation;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Attributes;

namespace CarShack.Hypermedia.Cars
{
    // this HTO has no route. Exists to demonstrate the use of ReturnDefaultRouteForUnknownHto.
    // It also demonstrates schema metadata: [Title] is the schema display name, the XML docs become the
    // schema description, and HtoTitle is the per-instance Siren title.
    /// <summary>A truck offered by the car shack.</summary>
    /// <remarks>Has no route; links to it resolve to the default route.</remarks>
    [Title("Truck")]
    [HypermediaObject(Classes = new[] { "Truck" })]
    public class HypermediaTruck : IHypermediaObject
    {
        public string? HtoTitle => $"Truck {this.Brand}";
        
        public string Brand { get; set; }

        public int Id { get; set; }

        public HypermediaTruck(string brand, int id)
        {
            this.Brand = brand;
            this.Id = id;
        }
    }
}