using AwesomeAssertions;
using RESTyard.Relations;

namespace RESTyard.AspNetCore.Extensions.Pagination.Test
{
    public class NavigationQueryBuilderTest
    {
        private const int DefaultPageSize = 10;

        [Fact]
        public void EmptyResult()
        {
            var query = new EntityQuery
            {
                Pagination = new RESTyard.Extensions.Pagination.Pagination(DefaultPageSize, 0)
            };

            var queryResult = new QueryResult<Entity> {TotalCountOfEntities = 0};

            var navigationQuerys = NavigationQueryBuilder.Build(query, queryResult);
            navigationQuerys.Queries.Should().HaveCount(0);
        }

        [Fact]
        public void LessThanPagesizeResult()
        {
            var query = new EntityQuery
            {
                Pagination = new RESTyard.Extensions.Pagination.Pagination(DefaultPageSize, 0)
            };

            var queryResult = new QueryResult<Entity> { TotalCountOfEntities = 8 };

            var navigationQuerys = NavigationQueryBuilder.Build(query, queryResult);
            navigationQuerys.Queries.Should().HaveCount(2);

            AssertAllQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.All]);
            AssertFirstQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.First]);
        }

        [Fact]
        public void ResultCountEqualPagesizeResult()
        {
            var query = new EntityQuery
            {
                Pagination = new RESTyard.Extensions.Pagination.Pagination(DefaultPageSize, 0)
            };

            var queryResult = new QueryResult<Entity> { TotalCountOfEntities = DefaultPageSize };

            var navigationQuerys = NavigationQueryBuilder.Build(query, queryResult);
            navigationQuerys.Queries.Should().HaveCount(2);

            AssertAllQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.All]);
            AssertFirstQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.First]);
        }

        [Fact]
        public void MoreThanPagesizeResult()
        {
            var pageOffset = 0;
            var query = new EntityQuery
            {
                Pagination = new RESTyard.Extensions.Pagination.Pagination(DefaultPageSize, pageOffset)
            };

            var queryResult = new QueryResult<Entity> { TotalCountOfEntities = 30 };

            var navigationQuerys = NavigationQueryBuilder.Build(query, queryResult);
            navigationQuerys.Queries.Should().HaveCount(4);
            AssertAllQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.All]);
            AssertFirstQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.First]);
            AssertNextQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.Next], pageOffset);
            AssertLastQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.Last], DefaultPageSize * ((queryResult.TotalCountOfEntities / 10) -1));
        }

        [Fact]
        public void MoreThanPagesizeNotFirstPageResult()
        {
            var pageOffset = 10;
            var query = new EntityQuery
            {
                Pagination = new RESTyard.Extensions.Pagination.Pagination(DefaultPageSize, pageOffset)
            };

            var queryResult = new QueryResult<Entity> { TotalCountOfEntities = 30 };

            var navigationQuerys = NavigationQueryBuilder.Build(query, queryResult);
            navigationQuerys.Queries.Should().HaveCount(5);
            AssertAllQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.All]);
            AssertFirstQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.First]);

            AssertPerviousQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.Previous], 0);

            AssertNextQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.Next], pageOffset);
            AssertLastQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.Last], DefaultPageSize * ((queryResult.TotalCountOfEntities / 10) - 1));
        }

        [Fact]
        public void MoreThanPagesizeNotFirstPageUnevenResult()
        {
            var pageOffset = 15;
            var query = new EntityQuery
            {
                Pagination = new RESTyard.Extensions.Pagination.Pagination(DefaultPageSize, pageOffset)
            };

            var queryResult = new QueryResult<Entity> { TotalCountOfEntities = 30 };

            var navigationQuerys = NavigationQueryBuilder.Build(query, queryResult);
            navigationQuerys.Queries.Should().HaveCount(5);
            AssertAllQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.All]);
            AssertFirstQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.First]);

            AssertPerviousQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.Previous], 5);

            AssertNextQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.Next], pageOffset);
            AssertLastQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.Last], DefaultPageSize * ((queryResult.TotalCountOfEntities / 10) - 1));
        }

        [Fact]
        public void MoreThanPagesizeNotFirstPageUnderflowResult()
        {
            var pageOffset = 5;
            var query = new EntityQuery
            {
                Pagination = new RESTyard.Extensions.Pagination.Pagination(DefaultPageSize, pageOffset)
            };

            var queryResult = new QueryResult<Entity> { TotalCountOfEntities = 30 };

            var navigationQuerys = NavigationQueryBuilder.Build(query, queryResult);
            navigationQuerys.Queries.Should().HaveCount(5);
            AssertAllQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.All]);
            AssertFirstQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.First]);

            AssertPerviousQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.Previous], 0);

            AssertNextQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.Next], pageOffset);
            AssertLastQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.Last], DefaultPageSize * ((queryResult.TotalCountOfEntities / 10) - 1));
        }


        [Fact]
        public void LastNotFirstPageResult()
        {
            var query = new EntityQuery
            {
                Pagination = new RESTyard.Extensions.Pagination.Pagination(DefaultPageSize, 20)
            };

            var queryResult = new QueryResult<Entity> { TotalCountOfEntities = 30 };

            var navigationQuerys = NavigationQueryBuilder.Build(query, queryResult);
            navigationQuerys.Queries.Should().HaveCount(4);
            AssertAllQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.All]);
            AssertFirstQuery((EntityQuery)navigationQuerys.Queries[DefaultHypermediaRelations.Queries.First]);
            navigationQuerys.Queries[DefaultHypermediaRelations.Queries.Previous].Should().NotBeNull();
            navigationQuerys.Queries[DefaultHypermediaRelations.Queries.Last].Should().NotBeNull();
        }

        private void AssertLastQuery(EntityQuery lastQuery, int correctPageOffset)
        {
            lastQuery.Should().NotBeNull();
            lastQuery.Pagination.PageSize.Should().Be(DefaultPageSize);
            lastQuery.Pagination.PageOffset.Should().Be(correctPageOffset);
        }

        private static void AssertAllQuery(EntityQuery allQuery)
        {
            allQuery.Should().NotBeNull();
            allQuery.Pagination.PageSize.Should().Be(0);
            allQuery.Pagination.PageOffset.Should().Be(0);
        }

        private void AssertFirstQuery(EntityQuery firstQuery)
        {
            firstQuery.Should().NotBeNull();
            firstQuery.Pagination.PageSize.Should().Be(DefaultPageSize);
            firstQuery.Pagination.PageOffset.Should().Be(0);
        }

        private void AssertNextQuery(EntityQuery nextQuery, int pageOffset)
        {
            nextQuery.Should().NotBeNull();
            nextQuery.Pagination.PageSize.Should().Be(DefaultPageSize);
            nextQuery.Pagination.PageOffset.Should().Be(pageOffset + DefaultPageSize);
        }

        private void AssertPerviousQuery(EntityQuery previousQuery, int correctPageOffset)
        {
            previousQuery.Should().NotBeNull();
            previousQuery.Pagination.PageSize.Should().Be(DefaultPageSize);
            previousQuery.Pagination.PageOffset.Should().Be(correctPageOffset);
        }
    }

#region MyRegion
    public record EntityQuery :HypermediaPaginationQuery<EntitySortProperties, EntityFilter>
    {
        public EntityQuery()
        {
        }

        public EntityQuery(EntityQuery customerQuery) : base(customerQuery)
        {
        }

        public override HypermediaPaginationQuery<EntitySortProperties, EntityFilter> DeepCopy()
        {
            return new EntityQuery(this);
        }
    }

    public class EntityFilter : IQueryFilter<EntityFilter>
    {
        public EntityFilter DeepCopy()
        {
            return new EntityFilter();
        }

        public static EntityFilter CreateDefault()
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
