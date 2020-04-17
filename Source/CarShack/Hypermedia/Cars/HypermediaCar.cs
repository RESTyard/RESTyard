using System.Collections.Generic;
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

        public IEnumerable<float> PriceDevelopment { get; set; }

        public List<Country> PopularCountries { get; set; }

        public Country MostPopularIn { get; set; }

        public HypermediaCar(string brand, int id)
        {
            this.Brand = brand;
            this.Id = id;

            this.PriceDevelopment = new float[] { 30000, 29000, 28100 };
            this.MostPopularIn = new Country {Name = "Germany", EstimatedPopulation = 80000000 };
            this.PopularCountries = new List<Country>()
            {
                new Country {Name = "Germany", EstimatedPopulation = 80000000},
                new Country {Name = "France", EstimatedPopulation = 67000000}
            };
        }
    }

    public class Country
    {
        public string Name { get; set; }

        // object properties can be attributed
        [Property(Name = "Population")]
        public int EstimatedPopulation { get; set; }

        [FormatterIgnore]
        public string LanguageCode { get; set; }
    }
}