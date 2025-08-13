using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarShack.Hypermedia;
using CarShack.Util.Repository;
using FunicularSwitch;
using RESTyard.AspNetCore.Extensions.Pagination;
using RESTyard.Extensions.Pagination;

namespace CarShack.Domain.Customer;

public interface ICustomerRepository : IRepository<Customer, int, CustomerQuery>;

public class CustomerRepository : RepositoryBase<Customer>, ICustomerRepository
{
    private List<Customer> CustomerList { get; } = CustomerService.GenerateRandomCustomersList(22);

    public Task<Result<Option<Customer>>> GetEntityByKeyAsync(int key)
    {
        return Task.FromResult(Result.Ok(CustomerList.FirstOrDefault(c => c.Id == key).ToOption()));
    }

    private static IOrderedQueryable<Customer> ApplyOrdering(
        IOrderedQueryable<Customer> query,
        Sorting<CustomerSortProperties> parameter) =>
        parameter.Order.Match(
            @default: () => query,
            ascending: () => parameter.Id.Match(
                age: () => query.ThenBy(c => c.Age),
                name: () => query.ThenBy(c => c.Name)),
            descending: () => parameter.Id.Match(
                age: () => query.ThenByDescending(c => c.Age),
                name: () => query.ThenByDescending(c => c.Name))
        );

    public Task<Result<IQueryResult<Customer>>> QueryAsync(CustomerQuery query)
    {
        // filter
        var filteredResults = CustomerList.Where(c => query.Filter.MinAge == null || c.Age >= query.Filter.MinAge)
            .ToList();
        var totalEntities = filteredResults.Count;

        var orderedResult = query.SortBy.Aggregate(
            filteredResults.AsQueryable().OrderBy(static _ => 0),
            ApplyOrdering
        );

        // pagination
        var resultEntities = ClipToPagination(orderedResult,
            query.Pagination);

        var result = new QueryResult<Customer>
        {
            Entities = resultEntities,
            TotalCountOfEntities = totalEntities
        };

        return Task.FromResult(Result.Ok<IQueryResult<Customer>>(result));
    }

    public Task<Result<Customer>> AddEntityAsync(Customer customer)
    {
        CustomerList.Add(customer);
        return Task.FromResult(Result.Ok(customer));
    }
}