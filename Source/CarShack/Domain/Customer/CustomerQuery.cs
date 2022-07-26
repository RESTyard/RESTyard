using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using RESTyard.WebApi.Extensions.Util.Repository;

namespace CarShack.Domain.Customer
{

    // Options for filtering
    public class CustomerFilter : IQueryFilter
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

        public IQueryFilter Clone()
        {
            return new CustomerFilter(this);
        }
    }
}