using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using CarShack.Domain.Customer;
using FunicularSwitch;
using Microsoft.Extensions.DependencyInjection;
using RESTyard.AspNetCore.Extensions.Pagination;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.AspNetCore.Hypermedia.Attributes;
using RESTyard.AspNetCore.Hypermedia.Links;
using RESTyard.AspNetCore.Util.Enum;
using RESTyard.Extensions.Pagination;

namespace CarShack.Hypermedia;

public partial record AddressTo
{
    public static AddressTo Create(Address domainAddress)
        => new(
            Street: domainAddress.Street,
            Number: domainAddress.Number,
            City: domainAddress.City,
            ZipCode: domainAddress.ZipCode);
}

public record CustomerQuery : HypermediaQueryBase<CustomerSortProperties, CustomerFilter>
{
    // required by Web Api to instanciate when a route is called
    public CustomerQuery()
    {
    }

    // copy constructor
    public CustomerQuery(CustomerQuery customerQuery) : base(customerQuery)
    {
    }

    public override HypermediaQueryBase<CustomerSortProperties, CustomerFilter> DeepCopy()
    {
        return new CustomerQuery(this);
    }
}

// Options for sorting
[TypeConverter(typeof(AttributedEnumTypeConverter<CustomerSortProperties>))]
public enum CustomerSortProperties
{
    [EnumMember(Value = "Age")] Age,

    [EnumMember(Value = "Name")] Name
}

// Options for filtering
public class CustomerFilter : IQueryFilter<CustomerFilter>
{
    public CustomerFilter()
    {
    }

    // copy constructor
    public CustomerFilter(CustomerFilter other)
    {
        MinAge = other.MinAge;
    }

    [Range(1, 150)] public int? MinAge { get; set; }

    public static CustomerFilter CreateDefault() => new();

    CustomerFilter IDeepCopyable<CustomerFilter>.DeepCopy() => new(this);
}

public class Country
{
    public string Name { get; set; }

    // object properties can be attributed
    [HypermediaProperty(Name = "Population")]
    public int EstimatedPopulation { get; set; }

    [FormatterIgnoreHypermediaProperty] public string LanguageCode { get; set; }
}

public partial class HypermediaEntrypointHto
{
   // public ExternalActionNoParametersTestOp ExternalActionNoParametersNoParametersTest { get; init; } = new ExternalActionNoParametersTestOp(new Uri("http://www.example1.com"), HttpMethod.POST);
   // public ExternalActionWitParameterTestOp ExternalActionWitParameterTestOp { get; init; }= new ExternalActionWitParameterTestOp(new Uri("http://www.example2.com"), HttpMethod.DELETE);
    
    //public List<object> Foo { get; set; } = new List<object>();

    public Type MyType { get; set; } = typeof(HypermediaEntrypointHto);
}

public partial class HypermediaCustomerHto
{
    public partial record CustomKey(int Key);
}

public class ExternalActionNoParametersTestOp : HypermediaExternalAction
{
    public ExternalActionNoParametersTestOp(Uri externalUri, string httpMethod) 
        : base(() => true, externalUri, httpMethod)
    {
    }
}

public class ExternalActionWitParameterTestOp : HypermediaExternalAction<ExternalActionParameters>
{
    public ExternalActionWitParameterTestOp(Uri externalUri, string httpMethod) 
        : base(() => true,
        externalUri,
        httpMethod,
        "application/json",
        new ExternalActionParameters(3))
    {
    }
}

public class ExternalActionParameters : IHypermediaActionParameter
{
    public int AInt { get; } = 4 ;

    public ExternalActionParameters(int aInt)
    {
        AInt = aInt;
    }
}

public partial class HypermediaCustomersRootHto
{
    [ActivatorUtilitiesConstructor]
    public HypermediaCustomersRootHto()
        : this(
            createCustomer: new CreateCustomerOp(() => true),
            createQuery: new CreateQueryOp(() => true,  new CustomerQuery
            {
                Filter = new CustomerFilter
                {
                    MinAge = 12
                },
                Pagination = new Pagination(10, 0),
                SortBy = new SortParameter<CustomerSortProperties>(CustomerSortProperties.Age, SortTypes.Ascending)
            }),
            allQuery: new CustomerQuery(),
            bestCustomerKey: new(1),
            greatSite: new HypermediaObjectReference(new ExternalReference(new Uri("https://www.example.com/")).WithAvailableMediaType("text/html")),
            okaySite: Option<HypermediaObjectReferenceBase>.None)
    {
    }
}

public partial class HypermediaCarsRootHto
{
    [ActivatorUtilitiesConstructor]
    public HypermediaCarsRootHto()
        : this(
            new UploadCarImageOp(
                () => true,
                new FileUploadConfiguration()
                {
                    Accept = [".jpg", "image/png", "image/*"],
                    AllowMultiple = false,
                    MaxFileSizeBytes = 1024 * 1024 * 4
                }),
            new UploadInsuranceScanOp(
                () => true,
                new FileUploadConfiguration()
                {
                    Accept = [".pdf"],
                    AllowMultiple = true,
                    MaxFileSizeBytes = 200,
                }),
            new DerivedCarHto.Key(Id: 2, Brand: "VW"),
            new HypermediaCarHto.Key(Id: 5, Brand: "Porsche"))
    {
    }
}

public partial class HypermediaCustomerHto
{
    public static HypermediaCustomerHto FromDomain(Customer customer)
    {
        var hto = new HypermediaCustomerHto(
            customer.Id,
            customer.Age,
            customer.Name,
            AddressTo.Create(customer.Address),
            customer.IsFavorite,
            new CustomerMoveOp(() => true),
            new CustomerRemoveOp(() => true),
            new MarkAsFavoriteOp(() => !customer.IsFavorite),
            new BuyCarOp(() => true, default));
        return hto;
    }
}

public partial class HypermediaCarHto
{
    public HypermediaCarHto(string brand, int carId) : this(carId, brand, Enumerable.Empty<float>(),
        new List<Country>(), default)
    {
    }
}

public static class HypermediaMappingExtensions
{
    public static HypermediaCustomerHto ToHto(this Customer customer)
    {
        return HypermediaCustomerHto.FromDomain(customer);
    }
}