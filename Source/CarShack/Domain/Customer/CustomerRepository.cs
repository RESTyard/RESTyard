using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarShack.Hypermedia;
using CarShack.Util.Repository;
using RESTyard.AspNetCore.Exceptions;
using RESTyard.AspNetCore.Util.Repository;

namespace CarShack.Domain.Customer
{
    public interface ICustomerRepository : IRepository<Customer, int, CustomerQuery> {
    }

    public class CustomerRepository : RepositoryBase<Customer>, ICustomerRepository
    {
        private List<Customer> CustomerList { get; }

        public CustomerRepository()
        {
            CustomerList = CustomerService.GenerateRandomCustomersList(22);
        }

        public Task<Customer> GetEntityByKeyAsync(int key)
        {
            var result = CustomerList.FirstOrDefault(c => c.Id == key);
            if (result == null)
            {
                throw new EntityNotFoundException($"EntityType: '{typeof(Customer)}', key: '{key}'");
            }

            return Task.FromResult(result);
        }

        public Task<QueryResult<Customer>> QueryAsync(CustomerQuery query)
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
                            throw new ArgumentOutOfRangeException();
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
                TotalCountOfEnties = totalEntities

            };

            return Task.FromResult(result);
        }

        public Task AddEntityAsync(Customer customer)
        {
            CustomerList.Add(customer);
            return Task.FromResult(0);
        }
    }
}
