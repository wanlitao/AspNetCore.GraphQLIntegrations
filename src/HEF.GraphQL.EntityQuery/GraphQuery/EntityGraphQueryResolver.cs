using GraphQL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HEF.GraphQL.EntityQuery
{
    public class EntityGraphQueryResolver : IEntityGraphQueryResolver
    {
        public EntityGraphQueryResolver(IEntityGraphQueryBuilderFactory entityGraphQueryBuilderFactory,
            IEnumerable<IEntityGraphQueryMiddlewareBuilder> entityGraphQueryMiddlewareBuilders)
        {
            EntityGraphQueryBuilderFactory = entityGraphQueryBuilderFactory
                ?? throw new ArgumentNullException(nameof(entityGraphQueryBuilderFactory));

            EntityGraphQueryMiddlewareBuilders = entityGraphQueryMiddlewareBuilders
                ?? throw new ArgumentNullException(nameof(entityGraphQueryMiddlewareBuilders));
        }

        protected IEntityGraphQueryBuilderFactory EntityGraphQueryBuilderFactory { get; }

        protected IEnumerable<IEntityGraphQueryMiddlewareBuilder> EntityGraphQueryMiddlewareBuilders { get; }

        public IQueryable<TEntity> Resolve<TEntity>(IResolveFieldContext resolveContext)
            where TEntity : class
        {
            var queryableBuilder = EntityGraphQueryBuilderFactory.Create<TEntity>();

            foreach (var middlewareBuilder in EntityGraphQueryMiddlewareBuilders)
            {
                queryableBuilder.Use(middlewareBuilder.Build<TEntity>(resolveContext));
            }

            return queryableBuilder.Build();
        }
    }
}
