using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using CarShack.Domain.Customer;
using Microsoft.Extensions.DependencyInjection;
using RESTyard.AspNetCore.Exceptions;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.AspNetCore.Hypermedia.Attributes;
using RESTyard.AspNetCore.Hypermedia.Links;
using RESTyard.AspNetCore.Util.Enum;
using RESTyard.AspNetCore.Util.Repository;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace CarShack.Hypermedia;

public class CustomerQuery : QueryBase<CustomerSortProperties, CustomerFilter>
{
    // required by Web Api to instanciate when a route is called
    public CustomerQuery()
    {
    }

    // copy constructor
    public CustomerQuery(CustomerQuery customerQuery) : base(customerQuery)
    {
    }

    public override QueryBase<CustomerSortProperties, CustomerFilter> Clone()
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
public class CustomerFilter : IQueryFilter
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

    public IQueryFilter Clone()
    {
        return new CustomerFilter(this);
    }
}

public class Country
{
    public string Name { get; set; }

    // object properties can be attributed
    [HypermediaProperty(Name = "Population")]
    public int EstimatedPopulation { get; set; }

    [FormatterIgnoreHypermediaProperty] public string LanguageCode { get; set; }
}

public record MarkAsFavoriteParameters
    ([property: KeyFromUri(typeof(HypermediaCustomerHto), "Customer")] int CustomerId) : IHypermediaActionParameter;

public partial class HypermediaEntrypointHto
{
   // public ExternalActionNoParametersTestOp ExternalActionNoParametersNoParametersTest { get; init; } = new ExternalActionNoParametersTestOp(new Uri("http://www.example1.com"), HttpMethod.POST);
   // public ExternalActionWitParameterTestOp ExternalActionWitParameterTestOp { get; init; }= new ExternalActionWitParameterTestOp(new Uri("http://www.example2.com"), HttpMethod.DELETE);
    
    //public List<object> Foo { get; set; } = new List<object>();

    public Type MyType { get; set; } = typeof(HypermediaEntrypointHto);
    
    
    [ActivatorUtilitiesConstructor]
    public HypermediaEntrypointHto(HypermediaCustomersRootHto customersRoot, HypermediaCarsRootHto carsRoot)
        : this(new HypermediaObjectReference(customersRoot), new HypermediaObjectReference(carsRoot))
    {
        //Foo = new List<object>() { 5, "wow" };
    }
}

public class ExternalActionNoParametersTestOp :HypermediaExternalAction
{
    public ExternalActionNoParametersTestOp(Uri externalUri, HttpMethod httpMethod) 
        : base(() => true, externalUri, httpMethod)
    {
    }
}

public class ExternalActionWitParameterTestOp :HypermediaExternalAction<ExternalActionParameters>
{
    public ExternalActionWitParameterTestOp(Uri externalUri,
        HttpMethod httpMethod) 
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
            new CreateCustomerOp(() => true),
            new CreateQueryOp(() => true,  new CustomerQuery
            {
                Filter = new CustomerFilter
                {
                    MinAge = 12
                },
                Pagination = new Pagination
                {
                    PageOffset = 0,
                    PageSize = 10
                },
                SortBy = new SortParameter<CustomerSortProperties>
                {
                    PropertyName = CustomerSortProperties.Age,
                    SortType = SortTypes.Ascending
                }
            }),
            new CustomerQuery(),
            default,
            1,
            new HypermediaObjectReference(new ExternalReference(new Uri("http://www.example.com/")).WithAvailableMediaType("text/html")))
    {
    }

   
}

public partial class HypermediaCarsRootHto
{
    [ActivatorUtilitiesConstructor]
    public HypermediaCarsRootHto()
        : this(
            new HypermediaObjectReference(new HypermediaCarHto("VW", 2)),
            new {Brand = "Porsche", Id = 5})
    {
    }
}

public partial class HypermediaCustomerHto
{
    public static HypermediaCustomerHto FromDomain(Customer customer)
    {
        HypermediaCustomerHto hto = default!;
        hto = new HypermediaCustomerHto(
            customer.Id,
            customer.Age,
            customer.Name,
            customer.Address,
            customer.IsFavorite,
            new CustomerMoveOp(() => true),
            new CustomerRemoveOp(() => true),
            new MarkAsFavoriteOp(() => !hto!.IsFavorite),
            new BuyCarOp(() => true, default!));
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

public partial class HypermediaCustomerQueryResultHto
{
    public HypermediaCustomerQueryResultHto(ICollection<HypermediaCustomerHto> entities, int totalEntities,
        CustomerQuery query) : this(totalEntities, entities.Count, entities, query)
    {
    }
}