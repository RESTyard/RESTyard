using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarShack.Hypermedia;
using CarShack.Util.Repository;
using FunicularSwitch;
using RESTyard.AspNetCore.Exceptions;
using RESTyard.AspNetCore.Extensions.Pagination;
using RESTyard.Extensions.Pagination;

namespace CarShack.Domain.Customer
{
    public interface ICustomerRepository : IRepository<Customer, int, CustomerQuery>
    {
    }

    public class CustomerRepository : RepositoryBase<Customer>, ICustomerRepository
    {
        private List<Customer> CustomerList { get; }

        public CustomerRepository()
        {
            CustomerList = CustomerService.GenerateRandomCustomersList(22);
        }

        public Task<Result<Option<Customer>>> GetEntityByKeyAsync(int key)
        {
            return Task.FromResult(Result.Ok(CustomerList.FirstOrDefault(c => c.Id == key).ToOption()));
        }

        public Task<Result<IQueryResult<Customer>>> QueryAsync(CustomerQuery query)
        {
            // filter
            var filteredResults = CustomerList.Where(c => query.Filter.MinAge == null || c.Age >= query.Filter.MinAge).ToList();
            var totalEntities = filteredResults.Count;

            // sort
            IEnumerable<Customer> orderedResult;
            switch (query.SortBy.SortType)
            {
                case SortTypes.Ascending:
                    switch (query.SortBy.PropertyName)
                    {
                        case CustomerSortProperties.Age:
                            orderedResult = filteredResults.OrderBy(c => c.Age);
                            break;
                        case CustomerSortProperties.Name:
                            orderedResult = filteredResults.OrderBy(c => c.Name);
                            break;
                        case null:
                            orderedResult = filteredResults;
                            break;
                        default:
                            return Task.FromResult(Result.Error<IQueryResult<Customer>>("Invalid sort property specified."));
                    }

                    break;
                case SortTypes.Descending:
                    switch (query.SortBy.PropertyName)
                    {
                        case CustomerSortProperties.Age:
                            orderedResult = filteredResults.OrderByDescending(c => c.Age);
                            break;
                        case CustomerSortProperties.Name:
                            orderedResult = filteredResults.OrderByDescending(c => c.Name);
                            break;
                        case null:
                            orderedResult = filteredResults;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                case SortTypes.None:
                    orderedResult = filteredResults;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            // pagination
            var resultEntities = ClipToPagination(orderedResult, query.Pagination);

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
}