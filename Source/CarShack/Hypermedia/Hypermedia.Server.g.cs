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
using RESTyard.AspNetCore.Hypermedia.Extensions;
using RESTyard.AspNetCore.Hypermedia.Links;
using RESTyard.AspNetCore.Query;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace CarShack.Hypermedia;
public static class MimeTypes
{
    public const string APPLICATION_VND_SIREN_JSON = "application/vnd.siren+json";
}

/// <summary>
/// Defines a base for HTO keys such that they can be passed to <see cref = "LinkGenerator"/> and their values be recognized
/// </summary>
public abstract record KeyBase() : IEnumerable<KeyValuePair<string, object?>>
{
    IEnumerator<KeyValuePair<string, object?>> IEnumerable<KeyValuePair<string, object?>>.GetEnumerator() => EnumerateKeysForLinkGeneration().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => EnumerateKeysForLinkGeneration().GetEnumerator();
    protected abstract IEnumerable<KeyValuePair<string, object?>> EnumerateKeysForLinkGeneration();
}

public partial record CreateCustomerParameters(string Name) : IHypermediaActionParameter;
public partial record BuyCarParameters(string Brand, int CarId, double? Price = default, double? HiddenProperty = default) : IHypermediaActionParameter;
public partial record BuyLamborghiniParameters(string Brand, int CarId, string Color, double? Price = default, double? HiddenProperty = default, int? OptionalProperty = default) : BuyCarParameters(Brand, CarId, Price, HiddenProperty), IHypermediaQuery, IHypermediaActionParameter;
public partial record BuyLamborghinettaParameters(string Brand, int CarId, string Color, int HorsePower, double? Price = default, double? HiddenProperty = default, int? OptionalProperty = default) : BuyLamborghiniParameters(Brand, CarId, Color, Price, HiddenProperty, OptionalProperty), IHypermediaQuery, IHypermediaActionParameter;
public partial record NewAddress(string Address) : IHypermediaActionParameter;
public partial record UploadCarImageParameters(string Text, bool Flag) : IHypermediaActionParameter;
public partial record MarkAsFavoriteParameters(Uri Customer) : IHypermediaActionParameter;
[HypermediaObject(Title = "Entry to the Rest API", Classes = new string[] { "Entrypoint" })]
public partial class HypermediaEntrypointHto : HypermediaObject
{
    public HypermediaEntrypointHto() : base(hasSelfLink: true)
    {
        this.Links.Add("CustomersRoot", new HypermediaObjectKeyReference(typeof(HypermediaCustomersRootHto), null));
        this.Links.Add("CarsRoot", new HypermediaObjectKeyReference(typeof(HypermediaCarsRootHto), null));
    }
}

[HypermediaObject(Title = "The Cars API", Classes = new string[] { "CarsRoot" })]
public partial class HypermediaCarsRootHto : HypermediaObject
{
    [HypermediaAction(Name = "UploadCarImage", Title = "Upload image for car")]
    public UploadCarImageOp UploadCarImage { get; set; }

    [HypermediaAction(Name = "UploadInsuranceScan", Title = "Upload scan of insurance for the car")]
    public UploadInsuranceScanOp UploadInsuranceScan { get; set; }

    public HypermediaCarsRootHto(UploadCarImageOp uploadCarImage, UploadInsuranceScanOp uploadInsuranceScan, DerivedCarHto.Key niceCarKey, HypermediaCarHto.Key superCarKey) : base(hasSelfLink: true)
    {
        this.UploadCarImage = uploadCarImage;
        this.UploadInsuranceScan = uploadInsuranceScan;
        this.Links.Add("NiceCar", new HypermediaObjectKeyReference(typeof(DerivedCarHto), niceCarKey));
        this.Links.Add("SuperCar", new HypermediaObjectKeyReference(typeof(HypermediaCarHto), superCarKey));
    }

    public partial class UploadCarImageOp : FileUploadHypermediaAction<UploadCarImageParameters>
    {
        public UploadCarImageOp(Func<bool> canExecuteUploadCarImage, FileUploadConfiguration? fileUploadConfiguration = null, UploadCarImageParameters? prefilledValues = default) : base(canExecuteUploadCarImage, fileUploadConfiguration, prefilledValues)
        {
        }
    }

    public partial class UploadInsuranceScanOp : FileUploadHypermediaAction
    {
        public UploadInsuranceScanOp(Func<bool> canExecuteUploadInsuranceScan, FileUploadConfiguration? fileUploadConfiguration = null) : base(canExecuteUploadInsuranceScan, fileUploadConfiguration)
        {
        }
    }
}

[HypermediaObject(Title = "A Car", Classes = new string[] { "Car" })]
public partial class HypermediaCarHto : HypermediaObject
{
    [Key("id")]
    public int? Id { get; set; }

    [Key("brand")]
    public string? Brand { get; set; }
    public IEnumerable<float>? PriceDevelopment { get; set; }
    public List<Country>? PopularCountries { get; set; }
    public Country? MostPopularIn { get; set; }

    public HypermediaCarHto(int? id, string? brand, IEnumerable<float>? priceDevelopment, List<Country>? popularCountries, Country? mostPopularIn) : base(hasSelfLink: true)
    {
        this.Id = id;
        this.Brand = brand;
        this.PriceDevelopment = priceDevelopment;
        this.PopularCountries = popularCountries;
        this.MostPopularIn = mostPopularIn;
    }

    public partial record Key(int? Id, string? Brand) : KeyBase
    {
        protected override IEnumerable<KeyValuePair<string, object?>> EnumerateKeysForLinkGeneration()
        {
            yield return new KeyValuePair<string, object?>("id", this.Id);
            yield return new KeyValuePair<string, object?>("brand", this.Brand);
        }
    }
}

[HypermediaObject(Title = "Image for a car", Classes = new string[] { "CarImage" })]
public partial class CarImageHto : HypermediaObject
{
    [Key("filename")]
    [FormatterIgnoreHypermediaProperty]
    public string? Filename { get; set; }

    public CarImageHto(string? filename) : base(hasSelfLink: true)
    {
        this.Filename = filename;
    }

    public partial record Key(string? Filename) : KeyBase
    {
        protected override IEnumerable<KeyValuePair<string, object?>> EnumerateKeysForLinkGeneration()
        {
            yield return new KeyValuePair<string, object?>("filename", this.Filename);
        }
    }
}

[HypermediaObject(Title = "Insurance scan for a car", Classes = new string[] { "CarInsurance" })]
public partial class CarInsuranceHto : HypermediaObject
{
    [Key("filename")]
    [FormatterIgnoreHypermediaProperty]
    public string? Filename { get; set; }

    public CarInsuranceHto(string? filename) : base(hasSelfLink: true)
    {
        this.Filename = filename;
    }

    public partial record Key(string? Filename) : KeyBase
    {
        protected override IEnumerable<KeyValuePair<string, object?>> EnumerateKeysForLinkGeneration()
        {
            yield return new KeyValuePair<string, object?>("filename", this.Filename);
        }
    }
}

[HypermediaObject(Title = "Derived Car", Classes = new string[] { "DerivedCar" })]
public partial class DerivedCarHto : HypermediaCarHto
{
    public string? DerivedProperty { get; set; }

    [HypermediaAction(Name = "DerivedOperation", Title = "Derived Operation")]
    public DerivedOperationOp DerivedOperation { get; set; }

    public DerivedCarHto(int? id, string? brand, IEnumerable<float>? priceDevelopment, List<Country>? popularCountries, Country? mostPopularIn, string? derivedProperty, DerivedOperationOp derivedOperation, IEnumerable<HypermediaCustomerHto> item, Option<HypermediaCustomerHto.Key> derivedLinkKey) : base(id, brand, priceDevelopment, popularCountries, mostPopularIn)
    {
        this.DerivedProperty = derivedProperty;
        this.DerivedOperation = derivedOperation;
        this.Entities.AddRange("item", item);
        derivedLinkKey.Match(some => this.Links.Add("DerivedLink", new HypermediaObjectKeyReference(typeof(HypermediaCustomerHto), some)));
    }

    public partial class DerivedOperationOp : HypermediaAction
    {
        public DerivedOperationOp(Func<bool> canExecuteDerivedOperation) : base(canExecuteDerivedOperation)
        {
        }
    }
}

[HypermediaObject(Title = "Derives from Derived Car", Classes = new string[] { "NextLevelDerivedCar" })]
public partial class NextLevelDerivedCarHto : DerivedCarHto
{
    public string? NextLevelDerivedProperty { get; set; }

    public NextLevelDerivedCarHto(int? id, string? brand, IEnumerable<float>? priceDevelopment, List<Country>? popularCountries, Country? mostPopularIn, string? derivedProperty, DerivedOperationOp derivedOperation, IEnumerable<HypermediaCustomerHto> item, Option<HypermediaCustomerHto.Key> derivedLinkKey, string? nextLevelDerivedProperty) : base(id, brand, priceDevelopment, popularCountries, mostPopularIn, derivedProperty, derivedOperation, item, derivedLinkKey)
    {
        this.NextLevelDerivedProperty = nextLevelDerivedProperty;
    }
}

[HypermediaObject(Title = "The Customers API", Classes = new string[] { "CustomersRoot" })]
public partial class HypermediaCustomersRootHto : HypermediaObject
{
    [HypermediaAction(Name = "CreateCustomer", Title = "Request creation of a new Customer.")]
    public CreateCustomerOp CreateCustomer { get; set; }

    [HypermediaAction(Name = "CreateQuery", Title = "Query the Customers collection.")]
    public CreateQueryOp CreateQuery { get; set; }

    public HypermediaCustomersRootHto(CreateCustomerOp createCustomer, CreateQueryOp createQuery, CustomerQuery allQuery, HypermediaCustomerHto.Key bestCustomerKey, HypermediaObjectReferenceBase greatSite, Option<HypermediaObjectReferenceBase> okaySite) : base(hasSelfLink: true)
    {
        this.CreateCustomer = createCustomer;
        this.CreateQuery = createQuery;
        this.Links.Add("all", new HypermediaObjectQueryReference(typeof(HypermediaCustomerQueryResultHto), allQuery));
        this.Links.Add("BestCustomer", new HypermediaObjectKeyReference(typeof(HypermediaCustomerHto), bestCustomerKey));
        this.Links.Add("GreatSite", greatSite);
        okaySite.Match(some => this.Links.Add("OkaySite", some));
    }

    public partial class CreateCustomerOp : HypermediaAction<CreateCustomerParameters>
    {
        public CreateCustomerOp(Func<bool> canExecuteCreateCustomer, CreateCustomerParameters? prefilledValues = default) : base(canExecuteCreateCustomer, prefilledValues)
        {
        }
    }

    public partial class CreateQueryOp : HypermediaAction<CustomerQuery>
    {
        public CreateQueryOp(Func<bool> canExecuteCreateQuery, CustomerQuery? prefilledValues = default) : base(canExecuteCreateQuery, prefilledValues)
        {
        }
    }
}

[HypermediaObject(Title = "", Classes = new string[] { "Customer" })]
public partial class HypermediaCustomerHto : HypermediaObject
{
    [Key("id")]
    [FormatterIgnoreHypermediaProperty]
    public int Id { get; set; }
    public int? Age { get; set; }
    public string? FullName { get; set; }
    public string? Address { get; set; }
    public bool IsFavorite { get; set; }

    [HypermediaAction(Name = "CustomerMove", Title = "A Customer moved to a new location.")]
    public CustomerMoveOp CustomerMove { get; set; }

    [HypermediaAction(Name = "CustomerRemove", Title = "Remove a Customer.")]
    public CustomerRemoveOp CustomerRemove { get; set; }

    [HypermediaAction(Name = "MarkAsFavorite", Title = "Marks a Customer as a favorite buyer.")]
    public MarkAsFavoriteOp MarkAsFavorite { get; set; }

    [HypermediaAction(Name = "BuyCar", Title = "Buy a car.")]
    public BuyCarOp BuyCar { get; set; }

    public HypermediaCustomerHto(int id, int? age, string? fullName, string? address, bool isFavorite, CustomerMoveOp customerMove, CustomerRemoveOp customerRemove, MarkAsFavoriteOp markAsFavorite, BuyCarOp buyCar) : base(hasSelfLink: true)
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

    public partial record Key(int Id) : KeyBase
    {
        protected override IEnumerable<KeyValuePair<string, object?>> EnumerateKeysForLinkGeneration()
        {
            yield return new KeyValuePair<string, object?>("id", this.Id);
        }
    }

    public partial class CustomerMoveOp : HypermediaAction<NewAddress>
    {
        public CustomerMoveOp(Func<bool> canExecuteCustomerMove, NewAddress? prefilledValues = default) : base(canExecuteCustomerMove, prefilledValues)
        {
        }
    }

    public partial class CustomerRemoveOp : HypermediaAction
    {
        public CustomerRemoveOp(Func<bool> canExecuteCustomerRemove) : base(canExecuteCustomerRemove)
        {
        }
    }

    public partial class MarkAsFavoriteOp : HypermediaAction<MarkAsFavoriteParameters>
    {
        public MarkAsFavoriteOp(Func<bool> canExecuteMarkAsFavorite, MarkAsFavoriteParameters? prefilledValues = default) : base(canExecuteMarkAsFavorite, prefilledValues)
        {
        }
    }

    public partial class BuyCarOp : HypermediaAction<BuyCarParameters>
    {
        public BuyCarOp(Func<bool> canExecuteBuyCar, BuyCarParameters? prefilledValues = default) : base(canExecuteBuyCar, prefilledValues)
        {
        }
    }
}

[HypermediaObject(Title = "Query result on Customer", Classes = new string[] { "CustomersQueryResult" })]
public partial class HypermediaCustomerQueryResultHto : HypermediaQueryResult
{
    public int? TotalEntities { get; set; }
    public int? CurrentEntitiesCount { get; set; }

    public HypermediaCustomerQueryResultHto(int? totalEntities, int? currentEntitiesCount, IEnumerable<HypermediaCustomerHto> customers, Option<IHypermediaQuery> nextQuery, Option<IHypermediaQuery> previousQuery, Option<IHypermediaQuery> lastQuery, Option<IHypermediaQuery> allQuery, IHypermediaQuery query) : base(query)
    {
        this.TotalEntities = totalEntities;
        this.CurrentEntitiesCount = currentEntitiesCount;
        this.Entities.AddRange("Customers", customers);
        nextQuery.Match(some => this.Links.Add("Next", new HypermediaObjectQueryReference(typeof(HypermediaCustomerQueryResultHto), some)));
        previousQuery.Match(some => this.Links.Add("Previous", new HypermediaObjectQueryReference(typeof(HypermediaCustomerQueryResultHto), some)));
        lastQuery.Match(some => this.Links.Add("Last", new HypermediaObjectQueryReference(typeof(HypermediaCustomerQueryResultHto), some)));
        allQuery.Match(some => this.Links.Add("All", new HypermediaObjectQueryReference(typeof(HypermediaCustomerQueryResultHto), some)));
    }
}

public static partial class KeyFromUriServiceExtensions
{
    public static HypermediaCarHto.Key GetHypermediaCarKeyFromUri(this IKeyFromUriService keyFromUriService, Uri uri) => keyFromUriService.GetKeyFromUri<HypermediaCarHto, HypermediaCarHto.Key>(uri);
    public static CarImageHto.Key GetCarImageKeyFromUri(this IKeyFromUriService keyFromUriService, Uri uri) => keyFromUriService.GetKeyFromUri<CarImageHto, CarImageHto.Key>(uri);
    public static CarInsuranceHto.Key GetCarInsuranceKeyFromUri(this IKeyFromUriService keyFromUriService, Uri uri) => keyFromUriService.GetKeyFromUri<CarInsuranceHto, CarInsuranceHto.Key>(uri);
    public static HypermediaCustomerHto.Key GetHypermediaCustomerKeyFromUri(this IKeyFromUriService keyFromUriService, Uri uri) => keyFromUriService.GetKeyFromUri<HypermediaCustomerHto, HypermediaCustomerHto.Key>(uri);
}