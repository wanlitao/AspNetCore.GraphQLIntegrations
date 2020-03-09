using HEF.Data.Query;
using System;

namespace HEF.GraphQL.EntityQuery
{
    public class EntityGraphQueryBuilderFactory : IEntityGraphQueryBuilderFactory
    {
        public EntityGraphQueryBuilderFactory(IAsyncQueryProvider queryProvider)
        {
            QueryProvider = queryProvider ?? throw new ArgumentNullException(nameof(queryProvider));
        }

        protected IAsyncQueryProvider QueryProvider { get; }

        public IEntityGraphQueryBuilder<TEntity> Create<TEntity>() where TEntity : class
        {
            return new EntityGraphQueryBuilder<TEntity>(QueryProvider);
        }
    }
}
