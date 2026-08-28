#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FunicularSwitch;
using Microsoft.AspNetCore.Routing;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.AspNetCore.Hypermedia.Attributes;
using RESTyard.AspNetCore.Hypermedia.Links;
using RESTyard.AspNetCore.Query;
using RESTyard.AspNetCore.WebApi.RouteResolver;
using RESTyard.Relations;

namespace CarShack.Hypermedia.Siren;
public class Action
{
    public required IReadOnlyCollection<string> @class { get; set; }
    public required string name { get; set; }
    public required string method { get; set; }
    public required System.Uri href { get; set; }
    public required string title { get; set; }
    public required string type { get; set; }
}

public class Link
{
    public required IReadOnlyCollection<string> @class { get; set; }
    public required string title { get; set; }
    public required IReadOnlyCollection<string> rel { get; set; }
    public required System.Uri href { get; set; }
    public required string type { get; set; }
}

public class EmbeddedEntity
{
    public required object properties { get; set; }
    public required ICollection<EmbeddedEntity> entities { get; set; }
    public required ICollection<Action> actions { get; set; }
    public required ICollection<Link> links { get; set; }
    public required IReadOnlyCollection<string> rel { get; set; }
}

public class HypermediaEntrypoint
{
    public IReadOnlyCollection<string> @class { get; set; } = ["Entrypoint"];
    public string title { get; set; } = "Entry to the Rest API";
    public required ICollection<EmbeddedEntity> entities { get; set; }
    public required ICollection<Action> actions { get; set; }
    public required ICollection<Link> links { get; set; }
}

public class EmbeddedHypermediaEntrypoint
{
    public IReadOnlyCollection<string> @class { get; set; } = ["Entrypoint"];
    public string title { get; set; } = "Entry to the Rest API";
    public required ICollection<EmbeddedEntity> entities { get; set; }
    public required ICollection<Action> actions { get; set; }
    public required ICollection<Link> links { get; set; }
    public required IReadOnlyCollection<string> rel { get; set; }
}

public class HypermediaCarsRoot
{
    public IReadOnlyCollection<string> @class { get; set; } = ["CarsRoot"];
    public string title { get; set; } = "The Cars API";
    public required ICollection<EmbeddedEntity> entities { get; set; }
    public required ICollection<Action> actions { get; set; }
    public required ICollection<Link> links { get; set; }
}

public class EmbeddedHypermediaCarsRoot
{
    public IReadOnlyCollection<string> @class { get; set; } = ["CarsRoot"];
    public string title { get; set; } = "The Cars API";
    public required ICollection<EmbeddedEntity> entities { get; set; }
    public required ICollection<Action> actions { get; set; }
    public required ICollection<Link> links { get; set; }
    public required IReadOnlyCollection<string> rel { get; set; }
}

public class HypermediaCar
{
    public IReadOnlyCollection<string> @class { get; set; } = ["Car"];
    public string title { get; set; } = "A Car";
    public required HypermediaCarProperties properties { get; set; }
    public required ICollection<EmbeddedEntity> entities { get; set; }
    public required ICollection<Action> actions { get; set; }
    public required ICollection<Link> links { get; set; }

    public partial class HypermediaCarProperties
    {
        public int? Id { get; set; }
        public string? Brand { get; set; }
        public IEnumerable<float>? PriceDevelopment { get; set; }
        public List<Country>? PopularCountries { get; set; }
        public Country? MostPopularIn { get; set; }
        public DateOnly? LastInspection { get; set; }
    }
}

public class EmbeddedHypermediaCar
{
    public IReadOnlyCollection<string> @class { get; set; } = ["Car"];
    public string title { get; set; } = "A Car";
    public required HypermediaCarProperties properties { get; set; }
    public required ICollection<EmbeddedEntity> entities { get; set; }
    public required ICollection<Action> actions { get; set; }
    public required ICollection<Link> links { get; set; }
    public required IReadOnlyCollection<string> rel { get; set; }

    public partial class HypermediaCarProperties
    {
        public int? Id { get; set; }
        public string? Brand { get; set; }
        public IEnumerable<float>? PriceDevelopment { get; set; }
        public List<Country>? PopularCountries { get; set; }
        public Country? MostPopularIn { get; set; }
        public DateOnly? LastInspection { get; set; }
    }
}

public class CarImage
{
    public IReadOnlyCollection<string> @class { get; set; } = ["CarImage"];
    public string title { get; set; } = "Image for a car";
    public required ICollection<EmbeddedEntity> entities { get; set; }
    public required ICollection<Action> actions { get; set; }
    public required ICollection<Link> links { get; set; }
}

public class EmbeddedCarImage
{
    public IReadOnlyCollection<string> @class { get; set; } = ["CarImage"];
    public string title { get; set; } = "Image for a car";
    public required ICollection<EmbeddedEntity> entities { get; set; }
    public required ICollection<Action> actions { get; set; }
    public required ICollection<Link> links { get; set; }
    public required IReadOnlyCollection<string> rel { get; set; }
}

public class CarInsurance
{
    public IReadOnlyCollection<string> @class { get; set; } = ["CarInsurance"];
    public string title { get; set; } = "Insurance scan for a car";
    public required ICollection<EmbeddedEntity> entities { get; set; }
    public required ICollection<Action> actions { get; set; }
    public required ICollection<Link> links { get; set; }
}

public class EmbeddedCarInsurance
{
    public IReadOnlyCollection<string> @class { get; set; } = ["CarInsurance"];
    public string title { get; set; } = "Insurance scan for a car";
    public required ICollection<EmbeddedEntity> entities { get; set; }
    public required ICollection<Action> actions { get; set; }
    public required ICollection<Link> links { get; set; }
    public required IReadOnlyCollection<string> rel { get; set; }
}

public class DerivedCar
{
    public IReadOnlyCollection<string> @class { get; set; } = ["DerivedCar"];
    public string title { get; set; } = "Derived Car";
    public required DerivedCarProperties properties { get; set; }
    public required ICollection<EmbeddedHypermediaCustomer> entities { get; set; }
    public required ICollection<Action> actions { get; set; }
    public required ICollection<Link> links { get; set; }

    public partial class DerivedCarProperties
    {
        public string? DerivedProperty { get; set; }
        public int? Id { get; set; }
        public string? Brand { get; set; }
        public IEnumerable<float>? PriceDevelopment { get; set; }
        public List<Country>? PopularCountries { get; set; }
        public Country? MostPopularIn { get; set; }
        public DateOnly? LastInspection { get; set; }
    }
}

public class EmbeddedDerivedCar
{
    public IReadOnlyCollection<string> @class { get; set; } = ["DerivedCar"];
    public string title { get; set; } = "Derived Car";
    public required DerivedCarProperties properties { get; set; }
    public required ICollection<EmbeddedHypermediaCustomer> entities { get; set; }
    public required ICollection<Action> actions { get; set; }
    public required ICollection<Link> links { get; set; }
    public required IReadOnlyCollection<string> rel { get; set; }

    public partial class DerivedCarProperties
    {
        public string? DerivedProperty { get; set; }
        public int? Id { get; set; }
        public string? Brand { get; set; }
        public IEnumerable<float>? PriceDevelopment { get; set; }
        public List<Country>? PopularCountries { get; set; }
        public Country? MostPopularIn { get; set; }
        public DateOnly? LastInspection { get; set; }
    }
}

public class NextLevelDerivedCar
{
    public IReadOnlyCollection<string> @class { get; set; } = ["NextLevelDerivedCar"];
    public string title { get; set; } = "Derives from Derived Car";
    public required NextLevelDerivedCarProperties properties { get; set; }
    public required ICollection<EmbeddedHypermediaCustomer> entities { get; set; }
    public required ICollection<Action> actions { get; set; }
    public required ICollection<Link> links { get; set; }

    public partial class NextLevelDerivedCarProperties
    {
        public string? NextLevelDerivedProperty { get; set; }
        public string? DerivedProperty { get; set; }
        public int? Id { get; set; }
        public string? Brand { get; set; }
        public IEnumerable<float>? PriceDevelopment { get; set; }
        public List<Country>? PopularCountries { get; set; }
        public Country? MostPopularIn { get; set; }
        public DateOnly? LastInspection { get; set; }
    }
}

public class EmbeddedNextLevelDerivedCar
{
    public IReadOnlyCollection<string> @class { get; set; } = ["NextLevelDerivedCar"];
    public string title { get; set; } = "Derives from Derived Car";
    public required NextLevelDerivedCarProperties properties { get; set; }
    public required ICollection<EmbeddedHypermediaCustomer> entities { get; set; }
    public required ICollection<Action> actions { get; set; }
    public required ICollection<Link> links { get; set; }
    public required IReadOnlyCollection<string> rel { get; set; }

    public partial class NextLevelDerivedCarProperties
    {
        public string? NextLevelDerivedProperty { get; set; }
        public string? DerivedProperty { get; set; }
        public int? Id { get; set; }
        public string? Brand { get; set; }
        public IEnumerable<float>? PriceDevelopment { get; set; }
        public List<Country>? PopularCountries { get; set; }
        public Country? MostPopularIn { get; set; }
        public DateOnly? LastInspection { get; set; }
    }
}

public class HypermediaCustomersRoot
{
    public IReadOnlyCollection<string> @class { get; set; } = ["CustomersRoot"];
    public string title { get; set; } = "The Customers API";
    public required ICollection<EmbeddedEntity> entities { get; set; }
    public required ICollection<Action> actions { get; set; }
    public required ICollection<Link> links { get; set; }
}

public class EmbeddedHypermediaCustomersRoot
{
    public IReadOnlyCollection<string> @class { get; set; } = ["CustomersRoot"];
    public string title { get; set; } = "The Customers API";
    public required ICollection<EmbeddedEntity> entities { get; set; }
    public required ICollection<Action> actions { get; set; }
    public required ICollection<Link> links { get; set; }
    public required IReadOnlyCollection<string> rel { get; set; }
}

public class CustomerPurchase
{
    public IReadOnlyCollection<string> @class { get; set; } = ["CustomerPurchase"];
    public string title { get; set; } = "";
    public required CustomerPurchaseProperties properties { get; set; }
    public required ICollection<EmbeddedEntity> entities { get; set; }
    public required ICollection<Action> actions { get; set; }
    public required ICollection<Link> links { get; set; }

    public partial class CustomerPurchaseProperties
    {
        public int? Amount { get; set; }
        public required string CardNumber { get; set; }
        public required string CardType { get; set; }
    }
}

public class EmbeddedCustomerPurchase
{
    public IReadOnlyCollection<string> @class { get; set; } = ["CustomerPurchase"];
    public string title { get; set; } = "";
    public required CustomerPurchaseProperties properties { get; set; }
    public required ICollection<EmbeddedEntity> entities { get; set; }
    public required ICollection<Action> actions { get; set; }
    public required ICollection<Link> links { get; set; }
    public required IReadOnlyCollection<string> rel { get; set; }

    public partial class CustomerPurchaseProperties
    {
        public int? Amount { get; set; }
        public required string CardNumber { get; set; }
        public required string CardType { get; set; }
    }
}

public class CustomerPurchaseHistory
{
    public IReadOnlyCollection<string> @class { get; set; } = ["CustomerPurchaseHistory"];
    public string title { get; set; } = "";
    public required ICollection<EmbeddedCustomerPurchase> entities { get; set; }
    public required ICollection<Action> actions { get; set; }
    public required ICollection<Link> links { get; set; }
}

public class EmbeddedCustomerPurchaseHistory
{
    public IReadOnlyCollection<string> @class { get; set; } = ["CustomerPurchaseHistory"];
    public string title { get; set; } = "";
    public required ICollection<EmbeddedCustomerPurchase> entities { get; set; }
    public required ICollection<Action> actions { get; set; }
    public required ICollection<Link> links { get; set; }
    public required IReadOnlyCollection<string> rel { get; set; }
}

public class HypermediaCustomer
{
    public IReadOnlyCollection<string> @class { get; set; } = ["Customer"];
    public string title { get; set; } = "";
    public required HypermediaCustomerProperties properties { get; set; }
    public required ICollection<EmbeddedEntity> entities { get; set; }
    public required ICollection<Action> actions { get; set; }
    public required ICollection<Link> links { get; set; }

    public partial class HypermediaCustomerProperties
    {
        public int? Age { get; set; }
        public string? FullName { get; set; }
        public AddressTo? Address { get; set; }
        public required bool IsFavorite { get; set; }
    }
}

public class EmbeddedHypermediaCustomer
{
    public IReadOnlyCollection<string> @class { get; set; } = ["Customer"];
    public string title { get; set; } = "";
    public required HypermediaCustomerProperties properties { get; set; }
    public required ICollection<EmbeddedEntity> entities { get; set; }
    public required ICollection<Action> actions { get; set; }
    public required ICollection<Link> links { get; set; }
    public required IReadOnlyCollection<string> rel { get; set; }

    public partial class HypermediaCustomerProperties
    {
        public int? Age { get; set; }
        public string? FullName { get; set; }
        public AddressTo? Address { get; set; }
        public required bool IsFavorite { get; set; }
    }
}

public class HypermediaCustomerQueryResult
{
    public IReadOnlyCollection<string> @class { get; set; } = ["CustomersQueryResult"];
    public string title { get; set; } = "Query result on Customer";
    public required HypermediaCustomerQueryResultProperties properties { get; set; }
    public required ICollection<EmbeddedHypermediaCustomer> entities { get; set; }
    public required ICollection<Action> actions { get; set; }
    public required ICollection<Link> links { get; set; }

    public partial class HypermediaCustomerQueryResultProperties
    {
        public int? TotalEntities { get; set; }
        public int? CurrentEntitiesCount { get; set; }
    }
}

public class EmbeddedHypermediaCustomerQueryResult
{
    public IReadOnlyCollection<string> @class { get; set; } = ["CustomersQueryResult"];
    public string title { get; set; } = "Query result on Customer";
    public required HypermediaCustomerQueryResultProperties properties { get; set; }
    public required ICollection<EmbeddedHypermediaCustomer> entities { get; set; }
    public required ICollection<Action> actions { get; set; }
    public required ICollection<Link> links { get; set; }
    public required IReadOnlyCollection<string> rel { get; set; }

    public partial class HypermediaCustomerQueryResultProperties
    {
        public int? TotalEntities { get; set; }
        public int? CurrentEntitiesCount { get; set; }
    }
}