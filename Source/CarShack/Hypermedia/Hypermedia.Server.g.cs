#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Bluehands.Hypermedia.Relations;
using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.Hypermedia.Actions;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;
using WebApi.HypermediaExtensions.Hypermedia.Extensions;
using WebApi.HypermediaExtensions.Hypermedia.Links;
using WebApi.HypermediaExtensions.Query;
using WebApi.HypermediaExtensions.WebApi.RouteResolver;

namespace CarShack.Hypermedia;

public static class MimeTypes
{
    public const string APPLICATION_VND_SIREN_JSON = "application/vnd.siren+json";
}

public partial record CreateCustomerParameters(string Name) : IHypermediaActionParameter;
public partial record BuyCarParameters(string Brand, int CarId, double? Price = default) : IHypermediaActionParameter;
public partial record NewAddress(string Address) : IHypermediaActionParameter;

[HypermediaObject(Title = "Entry to the Rest API", Classes = new string[]{ "Entrypoint" })]
public partial class HypermediaEntrypointHto : HypermediaObject
{
    public HypermediaEntrypointHto(
                HypermediaObjectReferenceBase customersRoot,
        HypermediaObjectReferenceBase carsRoot
    ) : base(hasSelfLink: true)
    {
        Links.Add("CustomersRoot", customersRoot);
        Links.Add("CarsRoot", carsRoot);
    }
}

[HypermediaObject(Title = "The Cars API", Classes = new string[]{ "CarsRoot" })]
public partial class HypermediaCarsRootHto : HypermediaObject
{
    public HypermediaCarsRootHto(
        HypermediaObjectReferenceBase niceCar,
        object? superCarKey
    ) : base(hasSelfLink: true)
    {
        Links.Add("NiceCar", niceCar);
        Links.Add("SuperCar", new HypermediaObjectKeyReference(typeof(HypermediaCarHto), superCarKey));
    }
}

[HypermediaObject(Title = "A Car", Classes = new string[]{ "Car" })]
public partial class HypermediaCarHto : HypermediaObject
{
    [Key("id")]
    public int?  Id { get; set; }

    [Key("brand")]
    public string?  Brand { get; set; }

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

public class CreateCustomer : HypermediaFunction<CreateCustomerParameters, HypermediaCustomerHto>
{
    public CreateCustomer(Func<bool> canExecuteCreateCustomer, Func<CreateCustomerParameters, HypermediaCustomerHto> executeCreateCustomer, CreateCustomerParameters? prefilledValues = default) : base(canExecuteCreateCustomer, executeCreateCustomer, prefilledValues) { }
}

public class CreateQuery : HypermediaAction<CustomerQuery>
{
    public CreateQuery(Func<bool> canExecuteCreateQuery, Action<CustomerQuery> executeCreateQuery, CustomerQuery? prefilledValues = default) : base(canExecuteCreateQuery, executeCreateQuery, prefilledValues) { }
}

[HypermediaObject(Title = "The Customers API", Classes = new string[]{ "CustomersRoot" })]
public partial class HypermediaCustomersRootHto : HypermediaObject
{
    [HypermediaAction(Name = "CreateCustomer", Title = "Request creation of a new Customer.")]
    public CreateCustomer CreateCustomer { get; init; }

    [HypermediaAction(Name = "CreateQuery", Title = "Query the Customers collection.")]
    public CreateQuery CreateQuery { get; init; }

    public HypermediaCustomersRootHto(
        CreateCustomer createCustomer,
        CreateQuery createQuery,
        CustomerQuery allQuery,
        object? allKey,
        object? bestCustomerKey,
        HypermediaObjectReferenceBase greatSite
    ) : base(hasSelfLink: true)
    {
        this.CreateCustomer = createCustomer;
        this.CreateQuery = createQuery;
        Links.Add("all", new HypermediaObjectQueryReference(typeof(HypermediaCustomerQueryResultHto), allQuery, allKey));
        Links.Add("BestCustomer", new HypermediaObjectKeyReference(typeof(HypermediaCustomerHto), bestCustomerKey));
        Links.Add("GreatSite", greatSite);
    }
}

public class CustomerMove : HypermediaAction<NewAddress>
{
    public CustomerMove(Func<bool> canExecuteCustomerMove, Action<NewAddress> executeCustomerMove, NewAddress? prefilledValues = default) : base(canExecuteCustomerMove, executeCustomerMove, prefilledValues) { }
}

public class MarkAsFavorite : HypermediaAction<MarkAsFavoriteParameters>
{
    public MarkAsFavorite(Func<bool> canExecuteMarkAsFavorite, Action<MarkAsFavoriteParameters> executeMarkAsFavorite, MarkAsFavoriteParameters? prefilledValues = default) : base(canExecuteMarkAsFavorite, executeMarkAsFavorite, prefilledValues) { }
}

public class BuyCar : HypermediaAction<BuyCarParameters>
{
    public BuyCar(Func<bool> canExecuteBuyCar, Action<BuyCarParameters> executeBuyCar, BuyCarParameters? prefilledValues = default) : base(canExecuteBuyCar, executeBuyCar, prefilledValues) { }
}

[HypermediaObject(Title = "", Classes = new string[]{  })]
public partial class HypermediaCustomerHto : HypermediaObject
{
    [FormatterIgnoreHypermediaProperty]
    public int Id { get; set; }

    public int? Age { get; set; }

    public string? FullName { get; set; }

    public string? Address { get; set; }

    public bool IsFavorite { get; set; }

    [HypermediaAction(Name = "CustomerMove", Title = "A Customer moved to a new location.")]
    public CustomerMove CustomerMove { get; init; }

    [HypermediaAction(Name = "MarkAsFavorite", Title = "Marks a Customer as a favorite buyer.")]
    public MarkAsFavorite MarkAsFavorite { get; init; }

    [HypermediaAction(Name = "BuyCar", Title = "Buy a car.")]
    public BuyCar BuyCar { get; init; }

    public HypermediaCustomerHto(
        int id,
        int? age,
        string? fullName,
        string? address,
        bool isFavorite,
        CustomerMove customerMove,
        MarkAsFavorite markAsFavorite,
        BuyCar buyCar
    ) : base(hasSelfLink: true)
    {
        this.Id = id;
        this.Age = age;
        this.FullName = fullName;
        this.Address = address;
        this.IsFavorite = isFavorite;
        this.CustomerMove = customerMove;
        this.MarkAsFavorite = markAsFavorite;
        this.BuyCar = buyCar;
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
        IEnumerable<HypermediaObjectReferenceBase> item,
        IHypermediaQuery query
    ) : base(query)
    {
        this.TotalEntities = totalEntities;
        this.CurrentEntitiesCount = currentEntitiesCount;
        Entities.AddRange("item", item);
    }
}