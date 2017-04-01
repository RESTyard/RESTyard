using WebApiHypermediaExtensionsCore.Hypermedia.Actions;
using WebApiHypermediaExtensionsCore.Query;

namespace WebApiHypermediaExtensionsCore.Util.Repository
{
    /// <summary>
    /// Parameter class to specify a Query.
    /// </summary>
    /// <typeparam name="TSortPropertyEnum">A enum listing all properties which can be sorted, no multiple sort supported</typeparam>
    /// <typeparam name="TQueryFilter">A class holding properties and the coresponding filter value.</typeparam>
    public abstract class QueryBase<TSortPropertyEnum, TQueryFilter> : IHypermediaActionParameter, IHypermediaQuery
        where TSortPropertyEnum : struct
        where TQueryFilter : IQueryFilter, new()
    {
        protected QueryBase()
        {
            SortBy = new SortParameter<TSortPropertyEnum>();
            Filter = new TQueryFilter();
            Pagination = new Pagination();
        }

        // The copy constructor.  
        protected QueryBase(QueryBase<TSortPropertyEnum, TQueryFilter> other)
        {
            Pagination = new Pagination(other.Pagination);
            SortBy = new SortParameter<TSortPropertyEnum>(other.SortBy);
            Filter = (TQueryFilter)other.Filter.Clone();
        }

        // Pagination parameters
        public Pagination Pagination { get; set; }

        // The property to sort by and sort order
        public SortParameter<TSortPropertyEnum> SortBy { get; set; }

        // Filter object for this query.
        public TQueryFilter Filter { get; set; }

        public abstract QueryBase<TSortPropertyEnum, TQueryFilter> Clone();
    }
}