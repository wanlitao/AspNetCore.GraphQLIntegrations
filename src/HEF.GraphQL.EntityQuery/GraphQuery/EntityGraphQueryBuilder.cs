using HEF.Data.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HEF.GraphQL.EntityQuery
{
    public class EntityGraphQueryBuilder<TEntity> : IEntityGraphQueryBuilder<TEntity>
        where TEntity : class
    {
        private readonly IList<Func<IQueryable<TEntity>, IQueryable<TEntity>>> _middlewares;

        public EntityGraphQueryBuilder(IAsyncQueryProvider queryProvider)
        {
            QueryProvider = queryProvider ?? throw new ArgumentNullException(nameof(queryProvider));
        }

        protected IAsyncQueryProvider QueryProvider { get; }

        public IQueryable<TEntity> Build()
        {
            IQueryable<TEntity> queryable = new DbEntityQueryable<TEntity>(QueryProvider);

            foreach (var component in _middlewares)
            {
                queryable = component(queryable);
            }

            return queryable;
        }

        public IEntityGraphQueryBuilder<TEntity> New()
        {
            return new EntityGraphQueryBuilder<TEntity>(QueryProvider);
        }

        public IEntityGraphQueryBuilder<TEntity> Use(Func<IQueryable<TEntity>, IQueryable<TEntity>> middleware)
        {
            _middlewares.Add(middleware);

            return this;
        }
    }
}
