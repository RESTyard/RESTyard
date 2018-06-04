using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;
using WebApi.HypermediaExtensions.WebApi.RouteResolver;

namespace CarShack.Hypermedia.Cars
{
    [HypermediaObject(Title = "A Car", Classes = new[] { "Car" })]
    public class HypermediaCar : HypermediaObject
    {
        // Marks property as part of the objects key so it is can be mapped to route parameters when creating links.
        [Key("brand")]
        public string Brand { get; set; }

        // Marks property as part of the objects key so it is can be mapped to route parameters when creating links
        [Key("key")]
        public int Id { get; set; }

        public HypermediaCar(string brand, int id)
        {
            this.Brand = brand;
            this.Id = id;
        }
    }
}