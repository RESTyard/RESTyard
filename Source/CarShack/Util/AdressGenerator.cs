using System;
using System.Collections.Generic;
using CarShack.Domain.Customer;

namespace CarShack.Util
{
    public class AdressGenerator
    {
        private readonly List<string> streets = new List<string>()
        {
            "Monroe Drive",
            "Eagle Street",
            "7th Street",
            "Sunset Avenue",
            "New Street",
            "Academy Street",
            "12th Street",
            "Race Street",
            "Hilltop Road",
            "Cambridge Road",
            "Strawberry Lane",
            "Lake Avenue",
            "Franklin Street",
            "Lantern Lane",
            "6th Avenue",
            "Route 17",
            "Country Club Drive",
            "Valley View Road",
            "4th Street North",
            "Magnolia Avenue"
        };

        private readonly Range numbers = 1..500;

        private readonly List<string> cities = new List<string>()
        {
            "Ahmedabad",
            "Bangalore",
            "Tianjin",
            "Johannesburg",
            "Delhi",
            "Shanghai",
            "Saint Petersburg",
            "Lahore",
            "Nairobi",
            "Addis Ababa",
            "Mumbai",
            "Istanbul",
            "Busan",
            "Kolkata",
            "Jaipur",
            "Cairo",
            "Riyadh",
            "Lima",
            "London",
            "Beijing"
        };

        private readonly Range zipCodes = 12345..56789;

        private readonly Random rand;

        public AdressGenerator(long seed = long.MinValue)
        {
            if (seed == long.MinValue)
            {
                seed = DateTime.Now.Ticks;
            }

            this.rand = new Random((int)seed);
        }

        public Address GenerateNext()
        {
            var street = streets[rand.Next(0, streets.Count - 1)];
            var number = rand.Next(numbers.Start.Value, numbers.End.Value);
            var city = cities[rand.Next(0, cities.Count - 1)];
            var zipCode = rand.Next(zipCodes.Start.Value, zipCodes.End.Value);
            return new Address(
                Street: street,
                Number: number.ToString(),
                City: city,
                ZipCode: zipCode.ToString());
        }
    }
}
