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

namespace CarShack.Hypermedia;
public static class MimeTypes
{
    public const string APPLICATION_VND_SIREN_JSON = "application/vnd.siren+json";
}

public partial record CreateCustomerParameters(string Name) : IHypermediaActionParameter;
public partial record BuyCarParameters(string Brand, int CarId, double? Price = default, double? HiddenProperty = default) : IHypermediaActionParameter;
public partial record BuyLamborghiniParameters(string Brand, int CarId, string Color, double? Price = default, double? HiddenProperty = default, int? OptionalProperty = default) : BuyCarParameters(Brand, CarId, Price, HiddenProperty), IHypermediaQuery, IHypermediaActionParameter;
public partial record BuyLamborghinettaParameters(string Brand, int CarId, string Color, int HorsePower, double? Price = default, double? HiddenProperty = default, int? OptionalProperty = default) : BuyLamborghiniParameters(Brand, CarId, Color, Price, HiddenProperty, OptionalProperty), IHypermediaQuery, IHypermediaActionParameter;
public partial record NewAddress(AddressTo Address) : IHypermediaActionParameter;
public partial record AddressTo(string Street, string Number, string City, string ZipCode);
public partial record UploadCarImageParameters(string Text, bool Flag) : IHypermediaActionParameter;
public partial record MarkAsFavoriteParameters(Uri Customer) : IHypermediaActionParameter;
public partial record CustomerPurchaseHistoryQuery(string? CardType = default) : IHypermediaQuery;
public partial record UpdateCarInspection(DateOnly NewInspection) : IHypermediaActionParameter;
[HypermediaObject(Title = "Entry to the Rest API", Classes = new string[] { "Entrypoint" })]
public partial class HypermediaEntrypointHto : IHypermediaObject
{
    [Relations(["CustomersRoot"])]
    public ILink<HypermediaCustomersRootHto> CustomersRoot { get; set; }

    [Relations(["CarsRoot"])]
    public ILink<HypermediaCarsRootHto> CarsRoot { get; set; }

    [Relations([DefaultHypermediaRelations.Self])]
    public ILink<HypermediaEntrypointHto> Self { get; set; }

    public HypermediaEntrypointHto(ILink<HypermediaCustomersRootHto> customersRoot, ILink<HypermediaCarsRootHto> carsRoot)
    {
        this.CustomersRoot = customersRoot;
        this.CarsRoot = carsRoot;
        this.Self = Link.To(this);
    }
}

[HypermediaObject(Title = "The Cars API", Classes = new string[] { "CarsRoot" })]
public partial class HypermediaCarsRootHto : IHypermediaObject
{
    [Relations(["NiceCar"])]
    public ILink<DerivedCarHto> NiceCar { get; set; }

    [Relations(["SuperCar"])]
    public ILink<HypermediaCarHto> SuperCar { get; set; }

    [Relations([DefaultHypermediaRelations.Self])]
    public ILink<HypermediaCarsRootHto> Self { get; set; }

    [HypermediaAction(Name = "UploadCarImage", Title = "Upload image for car")]
    public UploadCarImageOp UploadCarImage { get; set; }

    [HypermediaAction(Name = "UploadInsuranceScan", Title = "Upload scan of insurance for the car")]
    public UploadInsuranceScanOp UploadInsuranceScan { get; set; }

    public HypermediaCarsRootHto(UploadCarImageOp uploadCarImage, UploadInsuranceScanOp uploadInsuranceScan, ILink<DerivedCarHto> niceCar, ILink<HypermediaCarHto> superCar)
    {
        this.UploadCarImage = uploadCarImage;
        this.UploadInsuranceScan = uploadInsuranceScan;
        this.NiceCar = niceCar;
        this.SuperCar = superCar;
        this.Self = Link.To(this);
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
public partial class HypermediaCarHto : IHypermediaObject
{
    [Key("id")]
    public int? Id { get; set; }

    [Key("brand")]
    public string? Brand { get; set; }
    public IEnumerable<float>? PriceDevelopment { get; set; }
    public List<Country>? PopularCountries { get; set; }
    public Country? MostPopularIn { get; set; }
    public DateOnly? LastInspection { get; set; }

    [Relations([DefaultHypermediaRelations.Self])]
    public ILink<HypermediaCarHto> Self { get; set; }

    [HypermediaAction(Name = "UpdateInspection", Title = "")]
    public UpdateInspectionOp UpdateInspection { get; set; }

    public HypermediaCarHto(int? id, string? brand, IEnumerable<float>? priceDevelopment, List<Country>? popularCountries, Country? mostPopularIn, DateOnly? lastInspection, UpdateInspectionOp updateInspection)
    {
        this.Id = id;
        this.Brand = brand;
        this.PriceDevelopment = priceDevelopment;
        this.PopularCountries = popularCountries;
        this.MostPopularIn = mostPopularIn;
        this.LastInspection = lastInspection;
        this.UpdateInspection = updateInspection;
        this.Self = Link.To(this);
    }

    public partial record Key(int? Id, string? Brand) : HypermediaObjectKeyBase<HypermediaCarHto>
    {
        protected override IEnumerable<KeyValuePair<string, object?>> EnumerateKeysForLinkGeneration()
        {
            yield return new KeyValuePair<string, object?>("id", this.Id);
            yield return new KeyValuePair<string, object?>("brand", this.Brand);
        }
    }

    public partial class UpdateInspectionOp : HypermediaAction<UpdateCarInspection>
    {
        public UpdateInspectionOp(Func<bool> canExecuteUpdateInspection, UpdateCarInspection? prefilledValues = default) : base(canExecuteUpdateInspection, prefilledValues)
        {
        }
    }
}

[HypermediaObject(Title = "Image for a car", Classes = new string[] { "CarImage" })]
public partial class CarImageHto : IHypermediaObject
{
    [Key("filename")]
    [FormatterIgnoreHypermediaProperty]
    public string? Filename { get; set; }

    [Relations([DefaultHypermediaRelations.Self])]
    public ILink<CarImageHto> Self { get; set; }

    public CarImageHto(string? filename)
    {
        this.Filename = filename;
        this.Self = Link.To(this);
    }

    public partial record Key(string? Filename) : HypermediaObjectKeyBase<CarImageHto>
    {
        protected override IEnumerable<KeyValuePair<string, object?>> EnumerateKeysForLinkGeneration()
        {
            yield return new KeyValuePair<string, object?>("filename", this.Filename);
        }
    }
}

[HypermediaObject(Title = "Insurance scan for a car", Classes = new string[] { "CarInsurance" })]
public partial class CarInsuranceHto : IHypermediaObject
{
    [Key("filename")]
    [FormatterIgnoreHypermediaProperty]
    public string? Filename { get; set; }

    [Relations([DefaultHypermediaRelations.Self])]
    public ILink<CarInsuranceHto> Self { get; set; }

    public CarInsuranceHto(string? filename)
    {
        this.Filename = filename;
        this.Self = Link.To(this);
    }

    public partial record Key(string? Filename) : HypermediaObjectKeyBase<CarInsuranceHto>
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

    [Relations(["DerivedLink"])]
    public ILink<HypermediaCustomerHto>? DerivedLink { get; set; }

    [Relations(["item"])]
    public List<IEmbeddedEntity<HypermediaCustomerHto>> Item { get; set; }

    [Relations([DefaultHypermediaRelations.Self])]
    public new ILink<DerivedCarHto> Self { get; set; }

    [HypermediaAction(Name = "DerivedOperation", Title = "Derived Operation")]
    public DerivedOperationOp DerivedOperation { get; set; }

    public DerivedCarHto(int? id, string? brand, IEnumerable<float>? priceDevelopment, List<Country>? popularCountries, Country? mostPopularIn, DateOnly? lastInspection, UpdateInspectionOp updateInspection, string? derivedProperty, DerivedOperationOp derivedOperation, IEnumerable<HypermediaCustomerHto> item, Option<ILink<HypermediaCustomerHto>> derivedLink) : base(id, brand, priceDevelopment, popularCountries, mostPopularIn, lastInspection, updateInspection)
    {
        this.DerivedProperty = derivedProperty;
        this.DerivedOperation = derivedOperation;
        this.Item = item.Select(x => EmbeddedEntity.Embed<HypermediaCustomerHto>(x)).ToList();
        this.DerivedLink = derivedLink.GetValueOrDefault();
        this.Self = Link.To(this);
    }

    public new partial record Key(int? Id, string? Brand) : HypermediaObjectKeyBase<DerivedCarHto>
    {
        protected override IEnumerable<KeyValuePair<string, object?>> EnumerateKeysForLinkGeneration()
        {
            yield return new KeyValuePair<string, object?>("id", this.Id);
            yield return new KeyValuePair<string, object?>("brand", this.Brand);
        }
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

    [Relations([DefaultHypermediaRelations.Self])]
    public new ILink<NextLevelDerivedCarHto> Self { get; set; }

    public NextLevelDerivedCarHto(int? id, string? brand, IEnumerable<float>? priceDevelopment, List<Country>? popularCountries, Country? mostPopularIn, DateOnly? lastInspection, UpdateInspectionOp updateInspection, string? derivedProperty, DerivedOperationOp derivedOperation, IEnumerable<HypermediaCustomerHto> item, Option<ILink<HypermediaCustomerHto>> derivedLink, string? nextLevelDerivedProperty) : base(id, brand, priceDevelopment, popularCountries, mostPopularIn, lastInspection, updateInspection, derivedProperty, derivedOperation, item, derivedLink)
    {
        this.NextLevelDerivedProperty = nextLevelDerivedProperty;
        this.Self = Link.To(this);
    }

    public new partial record Key(int? Id, string? Brand) : HypermediaObjectKeyBase<NextLevelDerivedCarHto>
    {
        protected override IEnumerable<KeyValuePair<string, object?>> EnumerateKeysForLinkGeneration()
        {
            yield return new KeyValuePair<string, object?>("id", this.Id);
            yield return new KeyValuePair<string, object?>("brand", this.Brand);
        }
    }
}

[HypermediaObject(Title = "The Customers API", Classes = new string[] { "CustomersRoot" })]
public partial class HypermediaCustomersRootHto : IHypermediaObject
{
    [Relations(["all"])]
    public ILink<HypermediaCustomerQueryResultHto> All { get; set; }

    [Relations(["BestCustomer"])]
    public ILink<HypermediaCustomerHto> BestCustomer { get; set; }

    [Relations(["GreatSite"])]
    public ExternalLink GreatSite { get; set; }

    [Relations(["OkaySite"])]
    public ExternalLink? OkaySite { get; set; }

    [Relations([DefaultHypermediaRelations.Self])]
    public ILink<HypermediaCustomersRootHto> Self { get; set; }

    [HypermediaAction(Name = "CreateCustomer", Title = "Request creation of a new Customer.")]
    public CreateCustomerOp CreateCustomer { get; set; }

    [HypermediaAction(Name = "CreateQuery", Title = "Query the Customers collection.")]
    public CreateQueryOp CreateQuery { get; set; }

    public HypermediaCustomersRootHto(CreateCustomerOp createCustomer, CreateQueryOp createQuery, ILink<HypermediaCustomerQueryResultHto> all, ILink<HypermediaCustomerHto> bestCustomer, ExternalLink greatSite, Option<ExternalLink> okaySite)
    {
        this.CreateCustomer = createCustomer;
        this.CreateQuery = createQuery;
        this.All = all;
        this.BestCustomer = bestCustomer;
        this.GreatSite = greatSite;
        this.OkaySite = okaySite.GetValueOrDefault();
        this.Self = Link.To(this);
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

[HypermediaObject(Title = "", Classes = new string[] { "CustomerPurchase" })]
public partial class CustomerPurchaseHto : IHypermediaObject
{
    public int? Amount { get; set; }
    public string CardNumber { get; set; }
    public string CardType { get; set; }

    public CustomerPurchaseHto(int? amount, string cardNumber, string cardType)
    {
        this.Amount = amount;
        this.CardNumber = cardNumber;
        this.CardType = cardType;
    }
}

[HypermediaObject(Title = "", Classes = new string[] { "CustomerPurchaseHistory" })]
public partial class CustomerPurchaseHistoryHto : IHypermediaQueryResult
{
    [FormatterIgnoreHypermediaProperty]
    public IHypermediaQuery Query { get; set; }

    [Key("customerId")]
    [FormatterIgnoreHypermediaProperty]
    public int? CustomerId { get; set; }

    [Relations(["Purchases"])]
    public List<IEmbeddedEntity<CustomerPurchaseHto>> Purchases { get; set; }

    [Relations([DefaultHypermediaRelations.Self])]
    public ILink<CustomerPurchaseHistoryHto> Self { get; set; }

    public CustomerPurchaseHistoryHto(int? customerId, IEnumerable<CustomerPurchaseHto> purchases, IHypermediaQuery query)
    {
        this.CustomerId = customerId;
        this.Purchases = purchases.Select(x => EmbeddedEntity.Embed<CustomerPurchaseHto>(x)).ToList();
        this.Query = query;
        this.Self = Link.To(this);
    }

    public partial record Key(int? CustomerId) : HypermediaObjectKeyBase<CustomerPurchaseHistoryHto>
    {
        protected override IEnumerable<KeyValuePair<string, object?>> EnumerateKeysForLinkGeneration()
        {
            yield return new KeyValuePair<string, object?>("customerId", this.CustomerId);
        }
    }
}

[HypermediaObject(Title = "", Classes = new string[] { "Customer" })]
public partial class HypermediaCustomerHto : IHypermediaObject
{
    [Key("id")]
    [FormatterIgnoreHypermediaProperty]
    public int Id { get; set; }
    public int? Age { get; set; }
    public string? FullName { get; set; }
    public AddressTo? Address { get; set; }
    public bool IsFavorite { get; set; }

    [Relations(["PurchaseHistory"])]
    public ILink<CustomerPurchaseHistoryHto> PurchaseHistory { get; set; }

    [Relations([DefaultHypermediaRelations.Self])]
    public ILink<HypermediaCustomerHto> Self { get; set; }

    [HypermediaAction(Name = "CustomerMove", Title = "A Customer moved to a new location.")]
    public CustomerMoveOp CustomerMove { get; set; }

    [HypermediaAction(Name = "CustomerRemove", Title = "Remove a Customer.")]
    public CustomerRemoveOp CustomerRemove { get; set; }

    [HypermediaAction(Name = "MarkAsFavorite", Title = "Marks a Customer as a favorite buyer.")]
    public MarkAsFavoriteOp MarkAsFavorite { get; set; }

    [HypermediaAction(Name = "BuyCar", Title = "Buy a car.")]
    public BuyCarOp BuyCar { get; set; }

    public HypermediaCustomerHto(int id, int? age, string? fullName, AddressTo? address, bool isFavorite, CustomerMoveOp customerMove, CustomerRemoveOp customerRemove, MarkAsFavoriteOp markAsFavorite, BuyCarOp buyCar, ILink<CustomerPurchaseHistoryHto> purchaseHistory)
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
        this.PurchaseHistory = purchaseHistory;
        this.Self = Link.To(this);
    }

    public partial record Key(int Id) : HypermediaObjectKeyBase<HypermediaCustomerHto>
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
public partial class HypermediaCustomerQueryResultHto : IHypermediaQueryResult
{
    [FormatterIgnoreHypermediaProperty]
    public IHypermediaQuery Query { get; set; }
    public int? TotalEntities { get; set; }
    public int? CurrentEntitiesCount { get; set; }

    [Relations(["Next"])]
    public ILink<HypermediaCustomerQueryResultHto>? Next { get; set; }

    [Relations(["Previous"])]
    public ILink<HypermediaCustomerQueryResultHto>? Previous { get; set; }

    [Relations(["Last"])]
    public ILink<HypermediaCustomerQueryResultHto>? Last { get; set; }

    [Relations(["All"])]
    public ILink<HypermediaCustomerQueryResultHto>? All { get; set; }

    [Relations(["Customers"])]
    public List<IEmbeddedEntity<HypermediaCustomerHto>> Customers { get; set; }

    [Relations([DefaultHypermediaRelations.Self])]
    public ILink<HypermediaCustomerQueryResultHto> Self { get; set; }

    public HypermediaCustomerQueryResultHto(int? totalEntities, int? currentEntitiesCount, IEnumerable<HypermediaCustomerHto> customers, Option<ILink<HypermediaCustomerQueryResultHto>> next, Option<ILink<HypermediaCustomerQueryResultHto>> previous, Option<ILink<HypermediaCustomerQueryResultHto>> last, Option<ILink<HypermediaCustomerQueryResultHto>> all, IHypermediaQuery query)
    {
        this.TotalEntities = totalEntities;
        this.CurrentEntitiesCount = currentEntitiesCount;
        this.Customers = customers.Select(x => EmbeddedEntity.Embed<HypermediaCustomerHto>(x)).ToList();
        this.Next = next.GetValueOrDefault();
        this.Previous = previous.GetValueOrDefault();
        this.Last = last.GetValueOrDefault();
        this.All = all.GetValueOrDefault();
        this.Query = query;
        this.Self = Link.To(this);
    }
}

public static partial class KeyFromUriServiceExtensions
{
    public static Result<HypermediaCarHto.Key> GetHypermediaCarKeyFromUri(this IKeyFromUriService keyFromUriService, Uri uri) => keyFromUriService.GetKeyFromUri<HypermediaCarHto, HypermediaCarHto.Key>(uri);
    public static Result<CarImageHto.Key> GetCarImageKeyFromUri(this IKeyFromUriService keyFromUriService, Uri uri) => keyFromUriService.GetKeyFromUri<CarImageHto, CarImageHto.Key>(uri);
    public static Result<CarInsuranceHto.Key> GetCarInsuranceKeyFromUri(this IKeyFromUriService keyFromUriService, Uri uri) => keyFromUriService.GetKeyFromUri<CarInsuranceHto, CarInsuranceHto.Key>(uri);
    public static Result<CustomerPurchaseHistoryHto.Key> GetCustomerPurchaseHistoryKeyFromUri(this IKeyFromUriService keyFromUriService, Uri uri) => keyFromUriService.GetKeyFromUri<CustomerPurchaseHistoryHto, CustomerPurchaseHistoryHto.Key>(uri);
    public static Result<HypermediaCustomerHto.Key> GetHypermediaCustomerKeyFromUri(this IKeyFromUriService keyFromUriService, Uri uri) => keyFromUriService.GetKeyFromUri<HypermediaCustomerHto, HypermediaCustomerHto.Key>(uri);
}