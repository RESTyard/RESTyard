#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.AspNetCore.Hypermedia.Attributes;
using RESTyard.AspNetCore.Hypermedia.Links;
using RESTyard.AspNetCore.Query;
using RESTyard.AspNetCore.WebApi.RouteResolver;
using RESTyard.AspNetCore.Hypermedia.Extensions;

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
        HypermediaObjectReferenceBase greatSite
    ) : base(hasSelfLink: true)
    {
        this.CreateCustomer = createCustomer;
        this.CreateQuery = createQuery;
        Links.Add("all", new HypermediaObjectQueryReference(typeof(HypermediaCustomerQueryResultHto), allQuery, allKey));
        Links.Add("BestCustomer", new HypermediaObjectKeyReference(typeof(HypermediaCustomerHto), bestCustomerKey));
        Links.Add("GreatSite", greatSite);
    }

    public class CreateCustomerOp : HypermediaAction<CreateCustomerParameters>
    {
        public CreateCustomerOp(Func<bool> canExecuteCreateCustomer, CreateCustomerParameters? prefilledValues = default)
            : base(canExecuteCreateCustomer, prefilledValues) { }
    }

    public class CreateQueryOp : HypermediaAction<CustomerQuery>
    {
        public CreateQueryOp(Func<bool> canExecuteCreateQuery, CustomerQuery? prefilledValues = default)
            : base(canExecuteCreateQuery, prefilledValues) { }
    }
}

[HypermediaObject(Title = "", Classes = new string[]{ "Customer" })]
public partial class HypermediaCustomerHto : HypermediaObject
{
    [FormatterIgnoreHypermediaProperty]
    public int  Id { get; set; }

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

    public class CustomerMoveOp : HypermediaAction<NewAddress>
    {
        public CustomerMoveOp(Func<bool> canExecuteCustomerMove, NewAddress? prefilledValues = default)
            : base(canExecuteCustomerMove, prefilledValues) { }
    }

    public class CustomerRemoveOp : HypermediaAction
    {
        public CustomerRemoveOp(Func<bool> canExecuteCustomerRemove)
            : base(canExecuteCustomerRemove) { }
    }

    public class MarkAsFavoriteOp : HypermediaAction<MarkAsFavoriteParameters>
    {
        public MarkAsFavoriteOp(Func<bool> canExecuteMarkAsFavorite, MarkAsFavoriteParameters? prefilledValues = default)
            : base(canExecuteMarkAsFavorite, prefilledValues) { }
    }

    public class BuyCarOp : HypermediaAction<BuyCarParameters>
    {
        public BuyCarOp(Func<bool> canExecuteBuyCar, BuyCarParameters? prefilledValues = default)
            : base(canExecuteBuyCar, prefilledValues) { }
    }
}

[HypermediaObject(Title = "Query result on Customer", Classes = new string[]{ "CustomersQueryResult" })]
public partial class HypermediaCustomerQueryResultHto : HypermediaQueryResult
{
    public int?  TotalEntities { get; set; }

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
