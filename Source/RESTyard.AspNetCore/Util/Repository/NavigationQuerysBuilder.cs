﻿using System.Diagnostics.CodeAnalysis;
using FunicularSwitch;
using RESTyard.AspNetCore.Query;
using RESTyard.Relations;

namespace RESTyard.AspNetCore.Util.Repository
{
    public static class NavigationQuerysBuilder
    {
        public static NavigationQueries Build<TSortPropertyEnum, TQueryFilter, TEntity>(QueryBase<TSortPropertyEnum, TQueryFilter> query, QueryResult<TEntity> queryResult)
            where TSortPropertyEnum : struct
            where TQueryFilter : IQueryFilter, new()
        {
            var result = new NavigationQueries();
            if (!query.Pagination.HasPagination() || queryResult.TotalCountOfEnties <= 0)
            {
                return result;
            }

            result.AddQuery(DefaultHypermediaRelations.Queries.All, CreateQueryAll(query));

            if (TryCreateQueryFirst(query, queryResult.TotalCountOfEnties, out var createdQuery))
            {
                result.AddQuery(DefaultHypermediaRelations.Queries.First, createdQuery);
            }

            if (TryCreateQueryNext(query, queryResult.TotalCountOfEnties, out createdQuery))
            {
                result.AddQuery(DefaultHypermediaRelations.Queries.Next, createdQuery);
            }

            if (TryCreateQueryPrevious(query, queryResult.TotalCountOfEnties, out createdQuery))
            {
                result.AddQuery(DefaultHypermediaRelations.Queries.Previous, createdQuery);
            }

            if (TryCreateQueryLast(query, queryResult.TotalCountOfEnties, out createdQuery))
            {
                result.AddQuery(DefaultHypermediaRelations.Queries.Last, createdQuery);
            }


            return result;
        }

        public static (
            Option<QueryBase<TSortPropertyEnum, TQueryFilter>> all,
            Option<QueryBase<TSortPropertyEnum, TQueryFilter>> first,
            Option<QueryBase<TSortPropertyEnum, TQueryFilter>> next,
            Option<QueryBase<TSortPropertyEnum, TQueryFilter>> previous,
            Option<QueryBase<TSortPropertyEnum, TQueryFilter>> last)
            Create<TSortPropertyEnum, TQueryFilter, TEntity>(
                QueryBase<TSortPropertyEnum, TQueryFilter> query,
                QueryResult<TEntity> queryResult)
            where TSortPropertyEnum : struct
            where TQueryFilter : IQueryFilter, new()
        {
            var none = Option.None<QueryBase<TSortPropertyEnum, TQueryFilter>>();
            if (!query.Pagination.HasPagination() || queryResult.TotalCountOfEnties <= 0)
            {
                return (none, none, none, none, none);
            }

            return (
                CreateQueryAll(query),
                TryCreateQueryFirst(query, queryResult.TotalCountOfEnties, out var firstQuery) ? firstQuery : none,
                TryCreateQueryNext(query, queryResult.TotalCountOfEnties, out var nextQuery) ? nextQuery : none,
                TryCreateQueryPrevious(query, queryResult.TotalCountOfEnties, out var previousQuery) ? previousQuery : none,
                TryCreateQueryLast(query, queryResult.TotalCountOfEnties, out var lastQuery) ? lastQuery : none);
        }

        private static bool TryCreateQueryLast<TSortPropertyEnum, TQueryFilter>(
            QueryBase<TSortPropertyEnum, TQueryFilter> queryParameters,
            int queryResultCount,
            [NotNullWhen(true)] out QueryBase<TSortPropertyEnum, TQueryFilter>? queryLast)
            where TSortPropertyEnum : struct
            where TQueryFilter : IQueryFilter, new()
        {
            if (!HasLastPage(queryParameters, queryResultCount))
            {
                queryLast = null;
                return false;
            }

            queryLast = queryParameters.Clone();
            queryLast.Pagination.PageOffset = queryResultCount - queryParameters.Pagination.PageSize;
            if (queryLast.Pagination.PageOffset < 0)
            {
                queryLast.Pagination.PageOffset = 0;
            }

            return true;
        }

        private static bool TryCreateQueryPrevious<TSortPropertyEnum, TQueryFilter>(
            QueryBase<TSortPropertyEnum, TQueryFilter> queryParameters,
            int queryResultCount,
            [NotNullWhen(true)] out QueryBase<TSortPropertyEnum, TQueryFilter>? queryPrevious)
            where TSortPropertyEnum : struct
            where TQueryFilter : IQueryFilter, new()
        {
            if (!HasPreviousPage(queryParameters, queryResultCount))
            {
                queryPrevious = null;
                return false;
            }

            queryPrevious = queryParameters.Clone();
            queryPrevious.Pagination.PageOffset = queryParameters.Pagination.PageOffset - queryParameters.Pagination.PageSize;
            if (queryPrevious.Pagination.PageOffset < 0)
            {
                queryPrevious.Pagination.PageOffset = 0;
            }

            return true;
        }


        private static bool TryCreateQueryNext<TSortPropertyEnum, TQueryFilter>(
            QueryBase<TSortPropertyEnum, TQueryFilter> queryParameters,
            int queryResultCount,
            [NotNullWhen(true)] out QueryBase<TSortPropertyEnum, TQueryFilter>? queryNext)
            where TSortPropertyEnum : struct
            where TQueryFilter : IQueryFilter, new()
        {
            if (!HasNextPage(queryParameters, queryResultCount))
            {
                queryNext = null;
                return false;
            }

            queryNext = queryParameters.Clone();
            queryNext.Pagination.PageOffset = queryParameters.Pagination.PageOffset + queryParameters.Pagination.PageSize;

            return true;
        }

        private static bool TryCreateQueryFirst<TSortPropertyEnum, TQueryFilter>(
            QueryBase<TSortPropertyEnum, TQueryFilter> queryParameters,
            int queryResultCount,
            [NotNullWhen(true)] out QueryBase<TSortPropertyEnum, TQueryFilter>? queryFirst)
            where TSortPropertyEnum : struct
            where TQueryFilter : IQueryFilter, new()
            {
                if (!HasFirstPage(queryParameters, queryResultCount))
                {
                    queryFirst = null;
                    return false;
                }

                queryFirst = queryParameters.Clone();
                queryFirst.Pagination.PageOffset = 0;

                return true;
        }

        public static QueryBase<TSortPropertyEnum, TQueryFilter> CreateQueryAll<TSortPropertyEnum, TQueryFilter>(QueryBase<TSortPropertyEnum, TQueryFilter> queryParameters)
        where TSortPropertyEnum : struct
        where TQueryFilter : IQueryFilter, new()
        {
            var result = queryParameters.Clone();
            result.Pagination.DisablePagination();
            return result;
        }

        public static bool HasFirstPage<TSortPropertyEnum, TQueryFilter>(QueryBase<TSortPropertyEnum, TQueryFilter> query, int queryResultCount)
        where TSortPropertyEnum : struct
        where TQueryFilter : IQueryFilter, new()
        {
            return queryResultCount > 0;
        }

        public static bool HasNextPage<TSortPropertyEnum, TQueryFilter>(QueryBase<TSortPropertyEnum, TQueryFilter> query, int queryResultCount)
            where TSortPropertyEnum : struct
            where TQueryFilter : IQueryFilter, new()
        {
            return query.Pagination.PageOffset + query.Pagination.PageSize < queryResultCount;
        }

        public static bool HasPreviousPage<TSortPropertyEnum, TQueryFilter>(QueryBase<TSortPropertyEnum, TQueryFilter> query, int queryResultCount)
            where TSortPropertyEnum : struct
            where TQueryFilter : IQueryFilter, new()
        {
            return query.Pagination.PageOffset > 0;
        }

        public static bool HasLastPage<TSortPropertyEnum, TQueryFilter>(QueryBase<TSortPropertyEnum, TQueryFilter> query, int queryResultCount)
            where TSortPropertyEnum : struct
            where TQueryFilter : IQueryFilter, new()
        {
            return queryResultCount > query.Pagination.PageSize;
        }
    }
}