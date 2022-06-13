using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using CarShack.Domain.Customer;
using Microsoft.Extensions.DependencyInjection;
using WebApi.HypermediaExtensions.Exceptions;
using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.Hypermedia.Actions;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;
using WebApi.HypermediaExtensions.Hypermedia.Links;
using WebApi.HypermediaExtensions.Util.Enum;
using WebApi.HypermediaExtensions.Util.Repository;
using WebApi.HypermediaExtensions.WebApi.RouteResolver;

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
    [ActivatorUtilitiesConstructor]
    public HypermediaEntrypointHto(HypermediaCustomersRootHto customersRoot, HypermediaCarsRootHto carsRoot)
        : this(new HypermediaObjectReference(customersRoot), new HypermediaObjectReference(carsRoot))
    {
    }
}

public partial class HypermediaCustomersRootHto
{
    [ActivatorUtilitiesConstructor]
    public HypermediaCustomersRootHto(ICustomerRepository customerRepository)
        : this(
            new CreateCustomerOp(() => true, arg =>
            {
                var customer = CustomerService.CreateRandomCustomer();
                customer.Name = arg.Name;
                customerRepository.AddEntityAsync(customer).ConfigureAwait(false).GetAwaiter().GetResult();
                return customer.ToHto();
            }),
            new CreateQueryOp(() => true, _ => { }, new CustomerQuery
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
            new HypermediaObjectReference(new ExternalReference(new Uri("http://www.example.com/"))))
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
            new CustomerMoveOp(() => true, address => DoMove(hto!, customer, address)),
            new CustomerRemoveOp(() => true, () => { }),
            new MarkAsFavoriteOp(() => !hto!.IsFavorite, p => DoMarkAsFavorite(hto!, customer, p)),
            new BuyCarOp(() => true, default!));
        return hto;
    }

    private static void DoMarkAsFavorite(HypermediaCustomerHto hto, Customer customer,
        MarkAsFavoriteParameters markAsFavoriteParameters)
    {
        customer.IsFavorite = true;
        hto.IsFavorite = true;
    }

    private static void DoMove(HypermediaCustomerHto hto, Customer customer, NewAddress newAddress)
    {
        // semantic validation is busyness logic
        if (string.IsNullOrEmpty(newAddress.Address))
            throw new ActionParameterValidationException("New customer address may not be null or empty.");

        // call busyness logic here
        customer.Address = newAddress.Address;
        hto.Address = customer.Address;
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
    public HypermediaCustomerQueryResultHto(ICollection<HypermediaObjectReferenceBase> entities, int totalEntities,
        CustomerQuery query) : this(totalEntities, entities.Count, entities, query)
    {
    }
}