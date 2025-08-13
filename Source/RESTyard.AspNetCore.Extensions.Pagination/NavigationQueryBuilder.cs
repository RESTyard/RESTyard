using FunicularSwitch;
using RESTyard.AspNetCore.Query;
using RESTyard.Relations;

namespace RESTyard.AspNetCore.Extensions.Pagination
{
    /// <summary>
    /// Builder class to create navigation queries based on a query and the result of that query.
    /// </summary>
    public static class NavigationQueryBuilder
    {
        /// <summary>
        /// Builds a set of navigation queries for paginated data based on the provided query and its result.
        /// Creates navigation links for all, first, next, previous, and last pages when applicable.
        /// </summary>
        /// <param name="query">The original query containing pagination parameters and filter criteria</param>
        /// <param name="queryResult">The result of the executed query containing the total count of entities</param>
        /// <typeparam name="TSortPropertyEnum">The enum type used for sorting properties</typeparam>
        /// <typeparam name="TQueryFilter">The type of filter implementing <see cref="IQueryFilter{TInstance}"/></typeparam>
        /// <typeparam name="TEntity">The type of entity being queried</typeparam>
        /// <returns>A NavigationQueries object containing the generated navigation queries</returns>
        public static NavigationQueries Build<TSortPropertyEnum, TQueryFilter, TEntity>(
            IHypermediaPaginationQuery<TSortPropertyEnum, TQueryFilter> query,
            IQueryResult<TEntity> queryResult
        )
            where TSortPropertyEnum : struct
            where TQueryFilter : IQueryFilter<TQueryFilter>

        {
            var result = new NavigationQueries();
            if (query.Pagination.IsDisabled() || queryResult.TotalCountOfEntities <= 0)
            {
                return result;
            }

            result.AddQuery(DefaultHypermediaRelations.Queries.All, CreateQueryAll(query));

            TryCreateQueryFirst(query, queryResult.TotalCountOfEntities)
                .Match(firstQuery => result.AddQuery(DefaultHypermediaRelations.Queries.First, firstQuery));

            TryCreateQueryNext(query, queryResult.TotalCountOfEntities)
                .Match(nextQuery => result.AddQuery(DefaultHypermediaRelations.Queries.Next, nextQuery));

            TryCreateQueryPrevious(query, queryResult.TotalCountOfEntities)
                .Match(previousQuery => result.AddQuery(DefaultHypermediaRelations.Queries.Previous, previousQuery));

            TryCreateQueryLast(query, queryResult.TotalCountOfEntities)
                .Match(lastQuery => result.AddQuery(DefaultHypermediaRelations.Queries.Last, lastQuery));

            return result;
        }

        /// <summary>
        /// Builds a set of navigation queries based on the provided query and query result.
        /// Creates navigation links for all, first, next, previous, and last pages when applicable.
        /// </summary>
        /// <param name="query">The original query containing pagination parameters and filter criteria</param>
        /// <param name="queryResult">The result of the executed query containing the total count of entities</param>
        /// <typeparam name="TSortPropertyEnum">The enum type used for sorting properties</typeparam>
        /// <typeparam name="TQueryFilter">The type of filter implementing <see cref="IQueryFilter{TInstance}"/></typeparam>
        /// <typeparam name="TEntity">The type of entity being queried</typeparam>
        /// <returns>A tuple containing Options of navigation queries for all, first, next, previous, and last pages</returns>
        public static (
            Option<IHypermediaPaginationQuery<TSortPropertyEnum, TQueryFilter>> all,
            Option<IHypermediaPaginationQuery<TSortPropertyEnum, TQueryFilter>> first,
            Option<IHypermediaPaginationQuery<TSortPropertyEnum, TQueryFilter>> next,
            Option<IHypermediaPaginationQuery<TSortPropertyEnum, TQueryFilter>> previous,
            Option<IHypermediaPaginationQuery<TSortPropertyEnum, TQueryFilter>> last)
            Create<TSortPropertyEnum, TQueryFilter, TEntity>(
                IHypermediaPaginationQuery<TSortPropertyEnum, TQueryFilter> query,
                IQueryResult<TEntity> queryResult)
            where TSortPropertyEnum : struct
            where TQueryFilter : IQueryFilter<TQueryFilter>

        {
            var none = Option.None<IHypermediaPaginationQuery<TSortPropertyEnum, TQueryFilter>>();
            if (query.Pagination.IsDisabled() || queryResult.TotalCountOfEntities <= 0)
            {
                return (none, none, none, none, none);
            }

            return (
                all: Option.Some(CreateQueryAll(query)),
                first: TryCreateQueryFirst(query, queryResult.TotalCountOfEntities),
                next: TryCreateQueryNext(query, queryResult.TotalCountOfEntities),
                previous: TryCreateQueryPrevious(query, queryResult.TotalCountOfEntities),
                last: TryCreateQueryLast(query, queryResult.TotalCountOfEntities)
            );
        }

        private static Option<IHypermediaPaginationQuery<TSortPropertyEnum, TQueryFilter>> TryCreateQueryLast<TSortPropertyEnum, TQueryFilter>(
            IHypermediaPaginationQuery<TSortPropertyEnum, TQueryFilter> queryParameters,
            int queryResultCount)
            where TSortPropertyEnum : struct
            where TQueryFilter : IQueryFilter<TQueryFilter>

        {
            if (!HasLastPage(queryParameters, queryResultCount))
            {
                return Option.None<IHypermediaPaginationQuery<TSortPropertyEnum, TQueryFilter>>();
            }

            var queryLast = queryParameters.DeepCopy();
            var newOffset = queryResultCount - queryParameters.Pagination.PageSize;
            queryLast.Pagination = queryLast.Pagination with
            {
                PageOffset = newOffset < 0 ? 0 : newOffset
            };
            return Option.Some(queryLast);
        }

        private static Option<IHypermediaPaginationQuery<TSortPropertyEnum, TQueryFilter>> TryCreateQueryPrevious<TSortPropertyEnum, TQueryFilter>(
            IHypermediaPaginationQuery<TSortPropertyEnum, TQueryFilter> queryParameters,
            int queryResultCount)
            where TSortPropertyEnum : struct
            where TQueryFilter : IQueryFilter<TQueryFilter>

        {
            if (!HasPreviousPage(queryParameters, queryResultCount))
            {
                return Option.None<IHypermediaPaginationQuery<TSortPropertyEnum, TQueryFilter>>();
            }

            var queryPrevious = queryParameters.DeepCopy();
            var newOffset = queryParameters.Pagination.PageOffset - queryParameters.Pagination.PageSize;
            queryPrevious.Pagination = queryPrevious.Pagination with
            {
                PageOffset = newOffset < 0 ? 0 : newOffset
            };
            return Option.Some(queryPrevious);
        }


        private static Option<IHypermediaPaginationQuery<TSortPropertyEnum, TQueryFilter>> TryCreateQueryNext<TSortPropertyEnum, TQueryFilter>(
            IHypermediaPaginationQuery<TSortPropertyEnum, TQueryFilter> queryParameters,
            int queryResultCount)
            where TSortPropertyEnum : struct
            where TQueryFilter : IQueryFilter<TQueryFilter>

        {
            if (!HasNextPage(queryParameters, queryResultCount))
            {
                return Option.None<IHypermediaPaginationQuery<TSortPropertyEnum, TQueryFilter>>();
            }

            var queryNext = queryParameters.DeepCopy();
            queryNext.Pagination = queryNext.Pagination with
            {
                PageOffset = queryParameters.Pagination.PageOffset + queryParameters.Pagination.PageSize
            };
            return Option.Some(queryNext);
        }

        private static Option<IHypermediaPaginationQuery<TSortPropertyEnum, TQueryFilter>> TryCreateQueryFirst<TSortPropertyEnum, TQueryFilter>(
            IHypermediaPaginationQuery<TSortPropertyEnum, TQueryFilter> queryParameters,
            int queryResultCount)
            where TSortPropertyEnum : struct
            where TQueryFilter : IQueryFilter<TQueryFilter>

        {
            if (!HasFirstPage(queryParameters, queryResultCount))
            {
                return Option.None<IHypermediaPaginationQuery<TSortPropertyEnum, TQueryFilter>>();
            }

            var queryFirst = queryParameters.DeepCopy();
            queryFirst.Pagination = queryParameters.Pagination with
            {
                PageOffset = 0
            };
            return Option.Some(queryFirst);
        }

        /// <summary>
        /// Creates a query that retrieves all entities without pagination by disabling the pagination settings of the provided query.
        /// </summary>
        /// <returns>A copy with pagination disabled.</returns>
        public static IHypermediaPaginationQuery<TSortPropertyEnum, TQueryFilter> CreateQueryAll<TSortPropertyEnum, TQueryFilter>(
            IHypermediaPaginationQuery<TSortPropertyEnum, TQueryFilter> queryParameters)
            where TSortPropertyEnum : struct
            where TQueryFilter : IQueryFilter<TQueryFilter>

        {
            var result = queryParameters.DeepCopy();
            result.Pagination = RESTyard.Extensions.Pagination.Pagination.DisabledPagination;
            return result;
        }

        /// <summary>
        /// Checks if the query has a first page based on the query result count.
        /// </summary>
        public static bool HasFirstPage<TSortPropertyEnum, TQueryFilter>(
            IHypermediaPaginationQuery<TSortPropertyEnum, TQueryFilter> query,
            int queryResultCount)
            where TSortPropertyEnum : struct
            where TQueryFilter : IQueryFilter<TQueryFilter>

        {
            return queryResultCount > 0;
        }

        /// <summary>
        /// Checks if the query has a next page based on the query result count.
        /// </summary>
        public static bool HasNextPage<TSortPropertyEnum, TQueryFilter>(
            IHypermediaPaginationQuery<TSortPropertyEnum, TQueryFilter> query,
            int queryResultCount)
            where TSortPropertyEnum : struct
            where TQueryFilter : IQueryFilter<TQueryFilter>

        {
            return query.Pagination.PageOffset + query.Pagination.PageSize < queryResultCount;
        }

        /// <summary>
        /// Checks if the query has a previous page based on the current page offset.
        /// </summary>
        public static bool HasPreviousPage<TSortPropertyEnum, TQueryFilter>(
            IHypermediaPaginationQuery<TSortPropertyEnum, TQueryFilter> query,
            int queryResultCount)
            where TSortPropertyEnum : struct
            where TQueryFilter : IQueryFilter<TQueryFilter>

        {
            return query.Pagination.PageOffset > 0;
        }

        /// <summary>
        /// Checks if the query has a last page based on the query result count.
        /// </summary>
        public static bool HasLastPage<TSortPropertyEnum, TQueryFilter>(
            IHypermediaPaginationQuery<TSortPropertyEnum, TQueryFilter> query,
            int queryResultCount)
            where TSortPropertyEnum : struct
            where TQueryFilter : IQueryFilter<TQueryFilter>

        {
            return queryResultCount > query.Pagination.PageSize;
        }
    }
}