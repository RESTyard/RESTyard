using Hypermedia.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApiHypermediaExtensionsCore.Hypermedia;
using WebApiHypermediaExtensionsCore.Util.Repository;

namespace WebApiHypermediaExtensionsCore.Test
{
    [TestClass]
    public class NavigationQueryBuilderTest
    {
        private const int DefaultPageSize = 10;

        [TestMethod]
        public void EmptyResult()
        {
            var query = new EntityQuery
            {
                Pagination =
                {
                    PageOffset = 0,
                    PageSize = DefaultPageSize
                }
            };

            var queryResult = new QueryResult<Entity> {TotalCountOfEnties = 0};

            var navigationQuerys = NavigationQuerysBuilder.Build(query, queryResult);
            Assert.IsTrue(navigationQuerys.Queries.Count == 0);
        }

        [TestMethod]
        public void LessThanPagesizeResult()
        {
            var query = new EntityQuery
            {
                Pagination =
                {
                    PageOffset = 0,
                    PageSize = DefaultPageSize
                }
            };

            var queryResult = new QueryResult<Entity> { TotalCountOfEnties = 8 };

            var navigationQuerys = NavigationQuerysBuilder.Build(query, queryResult);
            Assert.IsTrue(navigationQuerys.Queries.Count == 2);

            AssertAllQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.All]);
            AssertFirstQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.First]);
        }

        [TestMethod]
        public void ResultCountEqualPagesizeResult()
        {
            var query = new EntityQuery
            {
                Pagination =
                {
                    PageOffset = 0,
                    PageSize = DefaultPageSize
                }
            };

            var queryResult = new QueryResult<Entity> { TotalCountOfEnties = DefaultPageSize };

            var navigationQuerys = NavigationQuerysBuilder.Build(query, queryResult);
            Assert.IsTrue(navigationQuerys.Queries.Count == 2);

            AssertAllQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.All]);
            AssertFirstQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.First]);
        }

        [TestMethod]
        public void MoreThanPagesizeResult()
        {
            var pageOffset = 0;
            var query = new EntityQuery
            {
                Pagination =
                {
                    PageOffset = pageOffset,
                    PageSize = DefaultPageSize
                }
            };

            var queryResult = new QueryResult<Entity> { TotalCountOfEnties = 30 };

            var navigationQuerys = NavigationQuerysBuilder.Build(query, queryResult);
            Assert.IsTrue(navigationQuerys.Queries.Count == 4);
            AssertAllQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.All]);
            AssertFirstQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.First]);
            AssertNextQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.Next], pageOffset);
            AssertLastQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.Last], DefaultPageSize * ((queryResult.TotalCountOfEnties / 10) -1));
        }

        [TestMethod]
        public void MoreThanPagesizeNotFirstPageResult()
        {
            var pageOffset = 10;
            var query = new EntityQuery
            {
                Pagination =
                {
                    PageOffset = pageOffset,
                    PageSize = DefaultPageSize
                }
            };

            var queryResult = new QueryResult<Entity> { TotalCountOfEnties = 30 };

            var navigationQuerys = NavigationQuerysBuilder.Build(query, queryResult);
            Assert.IsTrue(navigationQuerys.Queries.Count == 5);
            AssertAllQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.All]);
            AssertFirstQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.First]);

            AssertPerviousQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.Previous], 0);

            AssertNextQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.Next], pageOffset);
            AssertLastQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.Last], DefaultPageSize * ((queryResult.TotalCountOfEnties / 10) - 1));
        }

        [TestMethod]
        public void MoreThanPagesizeNotFirstPageUnevenResult()
        {
            var pageOffset = 15;
            var query = new EntityQuery
            {
                Pagination =
                {
                    PageOffset = pageOffset,
                    PageSize = DefaultPageSize
                }
            };

            var queryResult = new QueryResult<Entity> { TotalCountOfEnties = 30 };

            var navigationQuerys = NavigationQuerysBuilder.Build(query, queryResult);
            Assert.IsTrue(navigationQuerys.Queries.Count == 5);
            AssertAllQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.All]);
            AssertFirstQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.First]);

            AssertPerviousQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.Previous], 5);

            AssertNextQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.Next], pageOffset);
            AssertLastQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.Last], DefaultPageSize * ((queryResult.TotalCountOfEnties / 10) - 1));
        }

        [TestMethod]
        public void MoreThanPagesizeNotFirstPageUnderflowResult()
        {
            var pageOffset = 5;
            var query = new EntityQuery
            {
                Pagination =
                {
                    PageOffset = pageOffset,
                    PageSize = DefaultPageSize
                }
            };

            var queryResult = new QueryResult<Entity> { TotalCountOfEnties = 30 };

            var navigationQuerys = NavigationQuerysBuilder.Build(query, queryResult);
            Assert.IsTrue(navigationQuerys.Queries.Count == 5);
            AssertAllQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.All]);
            AssertFirstQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.First]);

            AssertPerviousQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.Previous], 0);

            AssertNextQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.Next], pageOffset);
            AssertLastQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.Last], DefaultPageSize * ((queryResult.TotalCountOfEnties / 10) - 1));
        }


        [TestMethod]
        public void LastNotFirstPageResult()
        {
            var query = new EntityQuery
            {
                Pagination =
                {
                    PageOffset = 20,
                    PageSize = DefaultPageSize
                }
            };

            var queryResult = new QueryResult<Entity> { TotalCountOfEnties = 30 };

            var navigationQuerys = NavigationQuerysBuilder.Build(query, queryResult);
            Assert.IsTrue(navigationQuerys.Queries.Count == 4);
            AssertAllQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.All]);
            AssertFirstQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.First]);
            Assert.IsTrue(navigationQuerys.Queries[DefaultHypermediaRelations.Queries.Previous] != null);
            Assert.IsTrue(navigationQuerys.Queries[DefaultHypermediaRelations.Queries.Last] != null);
        }

        private void AssertLastQuery(EntityQuery lastQuery, int correctPageOffset)
        {
            Assert.IsTrue(lastQuery != null);
            Assert.IsTrue(lastQuery.Pagination.PageSize == DefaultPageSize);
            Assert.IsTrue(lastQuery.Pagination.PageOffset == correctPageOffset);
        }

        private static void AssertAllQuery(EntityQuery allQuery)
        {
            Assert.IsTrue(allQuery != null);
            Assert.IsTrue(allQuery.Pagination.PageSize == 0);
            Assert.IsTrue(allQuery.Pagination.PageOffset == 0);
        }

        private void AssertFirstQuery(EntityQuery firstQuery)
        {
            Assert.IsTrue(firstQuery != null);
            Assert.IsTrue(firstQuery.Pagination.PageSize == DefaultPageSize);
            Assert.IsTrue(firstQuery.Pagination.PageOffset == 0);
        }

        private void AssertNextQuery(EntityQuery nextQuery, int pageOffset)
        {
            Assert.IsTrue(nextQuery != null);
            Assert.IsTrue(nextQuery.Pagination.PageSize == DefaultPageSize);
            Assert.IsTrue(nextQuery.Pagination.PageOffset == pageOffset + DefaultPageSize);
        }

        private void AssertPerviousQuery(EntityQuery previousQuery, int correctPageOffset)
        {
            Assert.IsTrue(previousQuery != null);
            Assert.IsTrue(previousQuery.Pagination.PageSize == DefaultPageSize);
            Assert.IsTrue(previousQuery.Pagination.PageOffset == correctPageOffset);
        }
    }

#region MyRegion
    public class EntityQuery :QueryBase<EntitySortProperties, EntityFilter>
    {
        public EntityQuery()
        {
        }

        public EntityQuery(EntityQuery customerQuery) : base(customerQuery)
        {
        }

        public override QueryBase<EntitySortProperties, EntityFilter> Clone()
        {
            return new EntityQuery(this);
        }
    }

    public class EntityFilter : IQueryFilter
    {
        public IQueryFilter Clone()
        {
            return new EntityFilter();
        }
    }

    public class Entity
    {
    }

    public enum EntitySortProperties
    {
        None,
    }
#endregion
}
