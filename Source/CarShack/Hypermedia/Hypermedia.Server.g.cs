#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.AspNetCore.Hypermedia.Attributes;
using RESTyard.AspNetCore.Hypermedia.Extensions;
using RESTyard.AspNetCore.Hypermedia.Links;
using RESTyard.AspNetCore.Query;
using RESTyard.AspNetCore.WebApi.RouteResolver;


namespace CarShack.Hypermedia;

public static class MimeTypes
{
    public const string APPLICATION_VND_SIREN_JSON = "application/vnd.siren+json";
}

public partial record CreateCustomerParameters(string Name) : IHypermediaActionParameter;
public partial record BuyCarParameters(string Brand, int CarId, double? Price = default, double? HiddenProperty = default) : IHypermediaActionParameter;
public partial record BuyLamborghiniParameters(string Brand, int CarId, string Color, double? Price = default, double? HiddenProperty = default, int? OptionalProperty = default) : BuyCarParameters(Brand, CarId, Price, HiddenProperty), IHypermediaQuery;
public partial record BuyLamborghinettaParameters(string Brand, int CarId, string Color, int HorsePower, double? Price = default, double? HiddenProperty = default, int? OptionalProperty = default) : BuyLamborghiniParameters(Brand, CarId, Color, Price, HiddenProperty, OptionalProperty), IHypermediaActionParameter;
public partial record NewAddress(string Address) : IHypermediaActionParameter;

[HypermediaObject(Title = "Entry to the Rest API", Classes = new string[]{ "Entrypoint" })]
public partial class HypermediaEntrypointHto : HypermediaObject
{
    public HypermediaEntrypointHto(
        object? customersRootKey,
        object? carsRootKey
    ) : base(hasSelfLink: true)
    {
        Links.Add("CustomersRoot", new HypermediaObjectKeyReference(typeof(HypermediaCustomersRootHto), customersRootKey));
        Links.Add("CarsRoot", new HypermediaObjectKeyReference(typeof(HypermediaCarsRootHto), carsRootKey));
    }
}

[HypermediaObject(Title = "The Cars API", Classes = new string[]{ "CarsRoot" })]
public partial class HypermediaCarsRootHto : HypermediaObject
{
    public HypermediaCarsRootHto(
        object? niceCarKey,
        object? superCarKey
    ) : base(hasSelfLink: true)
    {
        Links.Add("NiceCar", new HypermediaObjectKeyReference(typeof(DerivedCarHto), niceCarKey));
        Links.Add("SuperCar", new HypermediaObjectKeyReference(typeof(HypermediaCarHto), superCarKey));
    }
}

[HypermediaObject(Title = "A Car", Classes = new string[]{ "Car" })]
public partial class HypermediaCarHto : HypermediaObject
{
    [Key("id")]
    public int? Id { get; set; }

    [Key("brand")]
    public string? Brand { get; set; }

    public IEnumerable<float>? PriceDevelopment { get; set; }

    public List<Country>? PopularCountries { get; set; }

    public Country? MostPopularIn { get; set; }

    public HypermediaCarHto(
        int? id,
        string? brand,
        IEnumerable<float>? priceDevelopment,
        List<Country>? popularCountries,
        Country? mostPopularIn
    ) : base(hasSelfLink: true)
    {
        this.Id = id;
        this.Brand = brand;
        this.PriceDevelopment = priceDevelopment;
        this.PopularCountries = popularCountries;
        this.MostPopularIn = mostPopularIn;
    }
}

[HypermediaObject(Title = "Derived Car", Classes = new string[]{ "DerivedCar" })]
public partial class DerivedCarHto : HypermediaCarHto
{
    public string? DerivedProperty { get; set; }

    [HypermediaAction(Name = "Derived", Title = "Derived Operation")]
    public DerivedOp Derived { get; init; }

    public DerivedCarHto(
        int? id,
        string? brand,
        IEnumerable<float>? priceDevelopment,
        List<Country>? popularCountries,
        Country? mostPopularIn,
        string? derivedProperty,
        DerivedOp derived,
        IEnumerable<HypermediaCustomerHto> item,
        bool hasDerivedLink,
        object? derivedLinkKey
    ) : base(
            id,
            brand,
            priceDevelopment,
            popularCountries,
            mostPopularIn)
    {
        this.DerivedProperty = derivedProperty;
        this.Derived = derived;
        Entities.AddRange("item", item);
        if (hasDerivedLink)
        {
            Links.Add("DerivedLink", new HypermediaObjectKeyReference(typeof(HypermediaCustomerHto), derivedLinkKey));
        }
    }

    public partial class DerivedOp : HypermediaAction
    {
        public DerivedOp(Func<bool> canExecuteDerived)
            : base(canExecuteDerived) { }
    }
}

[HypermediaObject(Title = "Derives from Derived Car", Classes = new string[]{ "NextLevelDerivedCar" })]
public partial class NextLevelDerivedCarHto : DerivedCarHto
{
    public string? NextLevelDerivedProperty { get; set; }

    public NextLevelDerivedCarHto(
        int? id,
        string? brand,
        IEnumerable<float>? priceDevelopment,
        List<Country>? popularCountries,
        Country? mostPopularIn,
        string? derivedProperty,
        DerivedOp derived,
        IEnumerable<HypermediaCustomerHto> item,
        bool hasDerivedLink,
        object? derivedLinkKey,
        string? nextLevelDerivedProperty
    ) : base(
            id,
            brand,
            priceDevelopment,
            popularCountries,
            mostPopularIn,
            derivedProperty,
            derived,
            item,
            hasDerivedLink,
            derivedLinkKey)
    {
        this.NextLevelDerivedProperty = nextLevelDerivedProperty;
    }
}

[HypermediaObject(Title = "The Customers API", Classes = new string[]{ "CustomersRoot" })]
public partial class HypermediaCustomersRootHto : HypermediaObject
{
    [HypermediaAction(Name = "CreateCustomer", Title = "Request creation of a new Customer.")]
    public CreateCustomerOp CreateCustomer { get; init; }

    [HypermediaAction(Name = "CreateQuery", Title = "Query the Customers collection.")]
    public CreateQueryOp CreateQuery { get; init; }

    public HypermediaCustomersRootHto(
        CreateCustomerOp createCustomer,
        CreateQueryOp createQuery,
        CustomerQuery allQuery,
        object? allKey,
        object? bestCustomerKey,
        object? greatSiteKey
    ) : base(hasSelfLink: true)
    {
        this.CreateCustomer = createCustomer;
        this.CreateQuery = createQuery;
        Links.Add("all", new HypermediaObjectQueryReference(typeof(HypermediaCustomerQueryResultHto), allQuery, allKey));
        Links.Add("BestCustomer", new HypermediaObjectKeyReference(typeof(HypermediaCustomerHto), bestCustomerKey));
        Links.Add("GreatSite", new HypermediaObjectKeyReference(typeof(HypermediaCustomerHto), greatSiteKey));
    }

    public partial class CreateCustomerOp : HypermediaAction<CreateCustomerParameters>
    {
        public CreateCustomerOp(Func<bool> canExecuteCreateCustomer, CreateCustomerParameters? prefilledValues = default)
            : base(canExecuteCreateCustomer, prefilledValues) { }
    }

    public partial class CreateQueryOp : HypermediaAction<CustomerQuery>
    {
        public CreateQueryOp(Func<bool> canExecuteCreateQuery, CustomerQuery? prefilledValues = default)
            : base(canExecuteCreateQuery, prefilledValues) { }
    }
}

[HypermediaObject(Title = "", Classes = new string[]{ "Customer" })]
public partial class HypermediaCustomerHto : HypermediaObject
{
    [FormatterIgnoreHypermediaProperty]
    public int Id { get; set; }

    public int? Age { get; set; }

    public string? FullName { get; set; }

    public string? Address { get; set; }

    public bool IsFavorite { get; set; }

    [HypermediaAction(Name = "CustomerMove", Title = "A Customer moved to a new location.")]
    public CustomerMoveOp CustomerMove { get; init; }

    [HypermediaAction(Name = "CustomerRemove", Title = "Remove a Customer.")]
    public CustomerRemoveOp CustomerRemove { get; init; }

    [HypermediaAction(Name = "MarkAsFavorite", Title = "Marks a Customer as a favorite buyer.")]
    public MarkAsFavoriteOp MarkAsFavorite { get; init; }

    [HypermediaAction(Name = "BuyCar", Title = "Buy a car.")]
    public BuyCarOp BuyCar { get; init; }

    public HypermediaCustomerHto(
        int id,
        int? age,
        string? fullName,
        string? address,
        bool isFavorite,
        CustomerMoveOp customerMove,
        CustomerRemoveOp customerRemove,
        MarkAsFavoriteOp markAsFavorite,
        BuyCarOp buyCar
    ) : base(hasSelfLink: true)
    {
        this.Id = id;
        this.Age = age;
        this.FullName = fullName;
        this.Address = address;
        this.IsFavorite = isFavorite;
        this.CustomerMove = customerMove;
        this.CustomerRemove = customerRemove;
        this.MarkAsFavorite = markAsFavorite;
        this.BuyCar = buyCar;
    }

    public partial class CustomerMoveOp : HypermediaAction<NewAddress>
    {
        public CustomerMoveOp(Func<bool> canExecuteCustomerMove, NewAddress? prefilledValues = default)
            : base(canExecuteCustomerMove, prefilledValues) { }
    }

    public partial class CustomerRemoveOp : HypermediaAction
    {
        public CustomerRemoveOp(Func<bool> canExecuteCustomerRemove)
            : base(canExecuteCustomerRemove) { }
    }

    public partial class MarkAsFavoriteOp : HypermediaAction<MarkAsFavoriteParameters>
    {
        public MarkAsFavoriteOp(Func<bool> canExecuteMarkAsFavorite, MarkAsFavoriteParameters? prefilledValues = default)
            : base(canExecuteMarkAsFavorite, prefilledValues) { }
    }

    public partial class BuyCarOp : HypermediaAction<BuyCarParameters>
    {
        public BuyCarOp(Func<bool> canExecuteBuyCar, BuyCarParameters? prefilledValues = default)
            : base(canExecuteBuyCar, prefilledValues) { }
    }
}

[HypermediaObject(Title = "Query result on Customer", Classes = new string[]{ "CustomersQueryResult" })]
public partial class HypermediaCustomerQueryResultHto : HypermediaQueryResult
{
    public int? TotalEntities { get; set; }

    public int? CurrentEntitiesCount { get; set; }

    public HypermediaCustomerQueryResultHto(
        int? totalEntities,
        int? currentEntitiesCount,
        IEnumerable<HypermediaCustomerHto> item,
        IHypermediaQuery query
    ) : base(query)
    {
        this.TotalEntities = totalEntities;
        this.CurrentEntitiesCount = currentEntitiesCount;
        Entities.AddRange("item", item);
    }
}
