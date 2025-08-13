using AwesomeAssertions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using RESTyard.AspNetCore.Query;
using RESTyard.Extensions.Pagination;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace RESTyard.AspNetCore.Extensions.Pagination.Test;

public class QueryStringBuilderTest
{
    [Fact]
    public void QueryParameterSerializationShouldContainAllValues()
    {
        var query = new SamplePaginationQuery(
            new RESTyard.Extensions.Pagination.Pagination(3, 2),
            [new Sorting<SampleSortId>(SampleSortId.ValueA, SortOrder.Ascending)],
            new CustomerFilter { MaxValueA = 22 }
        );
        var queryStringBuilder = new QueryStringBuilder();
        var queryString = queryStringBuilder.CreateQueryString(query);

        var parsedQuery = QueryHelpers.ParseQuery(new Uri($"https://test.local{queryString}").Query);

        parsedQuery.Should().ContainInOrder([
            new KeyValuePair<string, StringValues>(
                $"{nameof(SamplePaginationQuery.Pagination)}.{nameof(RESTyard.Extensions.Pagination.Pagination.PageSize)}",
                "3"),
            new KeyValuePair<string, StringValues>(
                $"{nameof(SamplePaginationQuery.Pagination)}.{nameof(RESTyard.Extensions.Pagination.Pagination.PageOffset)}",
                "2"),
            // Omitted as it contains a default value
            // new KeyValuePair<string, StringValues>(
            //     $"{nameof(SampleQuery.SortBy)}[0].{nameof(SortParameter<SampleSortId>.Id)}",
            //     nameof(SampleSortId.ValueA)),
            new KeyValuePair<string, StringValues>(
                $"{nameof(SamplePaginationQuery.SortBy)}[0].{nameof(Sorting<SampleSortId>.Order)}",
                nameof(SortOrder.Ascending)),
            new KeyValuePair<string, StringValues>(
                $"{nameof(SamplePaginationQuery.Filter)}.{nameof(CustomerFilter.MaxValueA)}",
                "22")
        ]);
    }
    
    [Fact]
    public void QueryParameterSerializationShouldContainExplicitNullValue()
    {
        var query = new SamplePaginationQuery(
            new RESTyard.Extensions.Pagination.Pagination(3, 2),
            [new Sorting<SampleSortId>(SampleSortId.ValueA, SortOrder.Ascending)],
            new CustomerFilter()
        );
        var queryStringBuilder = new QueryStringBuilder();
        var queryString = queryStringBuilder.CreateQueryString(query);

        var parsedQuery = QueryHelpers.ParseQuery(new Uri($"https://test.local{queryString}").Query);

        parsedQuery.Should().ContainInOrder([
            new KeyValuePair<string, StringValues>(
                $"{nameof(SamplePaginationQuery.Pagination)}.{nameof(RESTyard.Extensions.Pagination.Pagination.PageSize)}",
                "3"),
            new KeyValuePair<string, StringValues>(
                $"{nameof(SamplePaginationQuery.Pagination)}.{nameof(RESTyard.Extensions.Pagination.Pagination.PageOffset)}",
                "2"),
            // Omitted as it contains a default value
            // new KeyValuePair<string, StringValues>(
            //     $"{nameof(SampleQuery.SortBy)}[0].{nameof(SortParameter<SampleSortId>.Id)}",
            //     nameof(SampleSortId.ValueA)),
            new KeyValuePair<string, StringValues>(
                $"{nameof(SamplePaginationQuery.SortBy)}[0].{nameof(Sorting<SampleSortId>.Order)}",
                nameof(SortOrder.Ascending)),
            // Omitted as it contains a default value
            // new KeyValuePair<string, StringValues>(
            //     $"{nameof(SampleQuery.Filter)}.{nameof(CustomerFilter.MaxValueA)}",
            //     "null")
        ]);
    }
}

public enum SampleSortId
{
    ValueA,
    ValueB
}

public record SamplePaginationQuery(
    RESTyard.Extensions.Pagination.Pagination Pagination,
    IReadOnlyCollection<Sorting<SampleSortId>> SortBy,
    CustomerFilter Filter) : PaginationQuery<SampleSortId, CustomerFilter>(Pagination, SortBy, Filter);

public class CustomerFilter : IDeepCopyable<CustomerFilter>
{
    public int? MaxValueA { get; set; }

    public CustomerFilter DeepCopy()
    {
        return new CustomerFilter { MaxValueA = MaxValueA };
    }
}