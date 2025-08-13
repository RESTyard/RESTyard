using System.Runtime.Serialization;
using RESTyard.Extensions.Pagination;

namespace RESTyard.Integration.Test.Hco;

public record CustomerPaginationQuery(
    Pagination Pagination,
    IReadOnlyCollection<Sorting<CustomerSortId>> SortBy,
    CustomerFilter Filter) : PaginationQuery<CustomerSortId, CustomerFilter>(Pagination, SortBy, Filter);


public enum CustomerSortId
{
    [EnumMember(Value = "Age")]
    Age
}

public class CustomerFilter : IDeepCopyable<CustomerFilter>
{
    public int MinAge { get; set; }

    public CustomerFilter DeepCopy()
    {
        return new CustomerFilter()
        {
            MinAge = MinAge
        };
    }
}