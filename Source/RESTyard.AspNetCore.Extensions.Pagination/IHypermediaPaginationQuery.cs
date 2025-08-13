using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.AspNetCore.Query;
using RESTyard.Extensions.Pagination;

namespace RESTyard.AspNetCore.Extensions.Pagination;

/// <summary>
/// Parameter class to specify a Query.
/// </summary>
/// <typeparam name="TSortPropertyEnum">An enum listing all properties that can be sorted, no multiple sort supported</typeparam>
/// <typeparam name="TQueryFilter">A class holding properties and the corresponding filter value.</typeparam>
public interface IHypermediaPaginationQuery<TSortPropertyEnum, TQueryFilter> : IHypermediaActionParameter, IHypermediaQuery where TSortPropertyEnum : struct
{
    /// <summary>
    /// Pagination parameters
    /// </summary>
    new RESTyard.Extensions.Pagination.Pagination Pagination { get; set; }

    /// <summary>
    /// The property to sort by and sort order
    /// </summary>
    new List<Sorting<TSortPropertyEnum>> SortBy { get; set; }

    /// <summary>
    /// Filter object for this query.
    /// </summary>
    new TQueryFilter Filter { get; set; }
    
    /// <summary>
    /// Has clone semantics, but clone is a reserved method for C# records.
    /// </summary>
    /// <returns></returns>
    IHypermediaPaginationQuery<TSortPropertyEnum, TQueryFilter> DeepCopy();
}