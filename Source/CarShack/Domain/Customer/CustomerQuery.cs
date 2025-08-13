using System.ComponentModel.DataAnnotations;
using RESTyard.AspNetCore.Extensions.Pagination;

namespace CarShack.Domain.Customer
{

    // Options for filtering
    public class CustomerFilter : IQueryFilter<CustomerFilter>
    {
        [Range(1, 150)]
        public int? MinAge { get; set; }

        public CustomerFilter()
        {
        }

        // copy constructor
        public CustomerFilter(CustomerFilter other)
        {
            MinAge = other.MinAge;
        }

        public CustomerFilter DeepCopy()
        {
            return new CustomerFilter(this);
        }

        public static CustomerFilter CreateDefault() => new CustomerFilter();
    }
}