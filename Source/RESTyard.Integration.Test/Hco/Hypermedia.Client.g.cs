#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RESTyard.Client;
using RESTyard.Client.Builder;
using RESTyard.Client.Hypermedia;
using RESTyard.Client.Hypermedia.Attributes;
using RESTyard.Client.Hypermedia.Commands;


namespace RESTyard.Integration.Test.Hco;

public class DefaultHypermediaClientBuilder
{
    public static IHypermediaResolverBuilder CreateBuilder()
        => HypermediaResolverBuilder.CreateBuilder()
            .ConfigureObjectRegister(register =>
            {
                register.Register<HypermediaEntrypointHco>();
                register.Register<HypermediaCarsRootHco>();
                register.Register<HypermediaCarHco>();
                register.Register<CarImageHco>();
                register.Register<CarInsuranceHco>();
                register.Register<DerivedCarHco>();
                register.Register<NextLevelDerivedCarHco>();
                register.Register<HypermediaCustomersRootHco>();
                register.Register<HypermediaCustomerHco>();
                register.Register<HypermediaCustomerQueryResultHco>();
            });
}
public partial record CreateCustomerParameters(string Name);
public partial record BuyCarParameters(string Brand, int CarId, double? Price = default);
public partial record BuyLamborghiniParameters(string Brand, int CarId, string Color, double? Price = default, int? OptionalProperty = default) : BuyCarParameters(Brand, CarId, Price);
public partial record BuyLamborghinettaParameters(string Brand, int CarId, string Color, int HorsePower, double? Price = default, int? OptionalProperty = default) : BuyLamborghiniParameters(Brand, CarId, Color, Price, OptionalProperty);
public partial record NewAddress(string Address);
public partial record UploadCarImageParameters(string Text, bool Flag);
public partial record MarkAsFavoriteParameters(Uri Customer);

[HypermediaClientObject("Entrypoint")]
public partial class HypermediaEntrypointHco : HypermediaClientObject
{
    [Mandatory]
    [HypermediaRelations(new[]{ "self" })]
    public MandatoryHypermediaLink<HypermediaEntrypointHco> Self { get; set; } = default!;

    [Mandatory]
    [HypermediaRelations(new[]{ "CustomersRoot" })]
    public MandatoryHypermediaLink<HypermediaCustomersRootHco> CustomersRoot { get; set; } = default!;

    [Mandatory]
    [HypermediaRelations(new[]{ "CarsRoot" })]
    public MandatoryHypermediaLink<HypermediaCarsRootHco> CarsRoot { get; set; } = default!;
}

[HypermediaClientObject("CarsRoot")]
public partial class HypermediaCarsRootHco : HypermediaClientObject
{
    [Mandatory]
    [HypermediaRelations(new[]{ "self" })]
    public MandatoryHypermediaLink<HypermediaCarsRootHco> Self { get; set; } = default!;

    [Mandatory]
    [HypermediaRelations(new[]{ "NiceCar" })]
    public MandatoryHypermediaLink<DerivedCarHco> NiceCar { get; set; } = default!;

    [Mandatory]
    [HypermediaRelations(new[]{ "SuperCar" })]
    public MandatoryHypermediaLink<HypermediaCarHco> SuperCar { get; set; } = default!;

    [HypermediaCommand("UploadCarImage")]
    public IHypermediaClientFileUploadFunction<CarImageHco, UploadCarImageParameters>? UploadCarImage { get; set; }

    [HypermediaCommand("UploadInsuranceScan")]
    public IHypermediaClientFileUploadFunction<CarInsuranceHco>? UploadInsuranceScan { get; set; }
}

[HypermediaClientObject("Car")]
public partial class HypermediaCarHco : HypermediaClientObject
{
    public int? Id { get; set; } = default!;

    public string? Brand { get; set; } = default!;

    public IEnumerable<float>? PriceDevelopment { get; set; } = default!;

    public List<Country>? PopularCountries { get; set; } = default!;

    public Country? MostPopularIn { get; set; } = default!;

    [Mandatory]
    [HypermediaRelations(new[]{ "self" })]
    public MandatoryHypermediaLink<HypermediaCarHco> Self { get; set; } = default!;
}

[HypermediaClientObject("CarImage")]
public partial class CarImageHco : HypermediaClientObject
{
    [Mandatory]
    [HypermediaRelations(new[]{ "self" })]
    public MandatoryHypermediaLink<CarImageHco> Self { get; set; } = default!;
}

[HypermediaClientObject("CarInsurance")]
public partial class CarInsuranceHco : HypermediaClientObject
{
    [Mandatory]
    [HypermediaRelations(new[]{ "self" })]
    public MandatoryHypermediaLink<CarInsuranceHco> Self { get; set; } = default!;
}

[HypermediaClientObject("DerivedCar")]
public partial class DerivedCarHco : HypermediaCarHco
{
    public string? DerivedProperty { get; set; } = default!;

    [Mandatory]
    [HypermediaRelations(new[]{ "self" })]
    public MandatoryHypermediaLink<DerivedCarHco> Self { get; set; } = default!;

    [HypermediaRelations(new[]{ "DerivedLink" })]
    public HypermediaLink<HypermediaCustomerHco>? DerivedLink { get; set; } = default!;

    [HypermediaRelations(new[]{ "item" })]
    public List<HypermediaCustomerHco> Item { get; set; } = default!;

    [HypermediaCommand("DerivedOperation")]
    public IHypermediaClientAction? DerivedOperation { get; set; }
}

[HypermediaClientObject("NextLevelDerivedCar")]
public partial class NextLevelDerivedCarHco : DerivedCarHco
{
    public string? NextLevelDerivedProperty { get; set; } = default!;

    [Mandatory]
    [HypermediaRelations(new[]{ "self" })]
    public MandatoryHypermediaLink<NextLevelDerivedCarHco> Self { get; set; } = default!;
}

[HypermediaClientObject("CustomersRoot")]
public partial class HypermediaCustomersRootHco : HypermediaClientObject
{
    [Mandatory]
    [HypermediaRelations(new[]{ "self" })]
    public MandatoryHypermediaLink<HypermediaCustomersRootHco> Self { get; set; } = default!;

    [Mandatory]
    [HypermediaRelations(new[]{ "all" })]
    public MandatoryHypermediaLink<HypermediaCustomerQueryResultHco> All { get; set; } = default!;

    [Mandatory]
    [HypermediaRelations(new[]{ "BestCustomer" })]
    public MandatoryHypermediaLink<HypermediaCustomerHco> BestCustomer { get; set; } = default!;

    [Mandatory]
    [HypermediaRelations(new[]{ "GreatSite" })]
    public MandatoryHypermediaLink<HypermediaClientObject> GreatSite { get; set; } = default!;

    [HypermediaRelations(new[]{ "OkaySite" })]
    public HypermediaLink<HypermediaClientObject>? OkaySite { get; set; } = default!;

    [HypermediaCommand("CreateCustomer")]
    public IHypermediaClientFunction<HypermediaCustomerHco, CreateCustomerParameters>? CreateCustomer { get; set; }

    [HypermediaCommand("CreateQuery")]
    public IHypermediaClientAction<CustomerQuery>? CreateQuery { get; set; }
}

[HypermediaClientObject("Customer")]
public partial class HypermediaCustomerHco : HypermediaClientObject
{
    public int? Age { get; set; } = default!;

    public string? FullName { get; set; } = default!;

    public string? Address { get; set; } = default!;

    [Mandatory]
    public bool IsFavorite { get; set; } = default!;

    [Mandatory]
    [HypermediaRelations(new[]{ "self" })]
    public MandatoryHypermediaLink<HypermediaCustomerHco> Self { get; set; } = default!;

    [HypermediaCommand("CustomerMove")]
    public IHypermediaClientAction<NewAddress>? CustomerMove { get; set; }

    [HypermediaCommand("CustomerRemove")]
    public IHypermediaClientAction? CustomerRemove { get; set; }

    [HypermediaCommand("MarkAsFavorite")]
    public IHypermediaClientAction<MarkAsFavoriteParameters>? MarkAsFavorite { get; set; }

    [HypermediaCommand("BuyCar")]
    public IHypermediaClientFunction<HypermediaCarHco, BuyCarParameters>? BuyCar { get; set; }
}

[HypermediaClientObject("CustomersQueryResult")]
public partial class HypermediaCustomerQueryResultHco : HypermediaClientObject
{
    public int? TotalEntities { get; set; } = default!;

    public int? CurrentEntitiesCount { get; set; } = default!;

    [Mandatory]
    [HypermediaRelations(new[]{ "self" })]
    public MandatoryHypermediaLink<HypermediaCustomerQueryResultHco> Self { get; set; } = default!;

    [HypermediaRelations(new[]{ "Next" })]
    public HypermediaLink<HypermediaCustomerQueryResultHco>? Next { get; set; } = default!;

    [HypermediaRelations(new[]{ "Previous" })]
    public HypermediaLink<HypermediaCustomerQueryResultHco>? Previous { get; set; } = default!;

    [HypermediaRelations(new[]{ "Last" })]
    public HypermediaLink<HypermediaCustomerQueryResultHco>? Last { get; set; } = default!;

    [HypermediaRelations(new[]{ "All" })]
    public HypermediaLink<HypermediaCustomerQueryResultHco>? All { get; set; } = default!;

    [HypermediaRelations(new[]{ "Customers" })]
    public List<HypermediaCustomerHco> Customers { get; set; } = default!;
}
