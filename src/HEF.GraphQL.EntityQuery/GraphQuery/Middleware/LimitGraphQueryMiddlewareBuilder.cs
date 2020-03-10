using GraphQL;
using System;
using System.Linq;

namespace HEF.GraphQL.EntityQuery
{
    public class LimitGraphQueryMiddlewareBuilder : IEntityGraphQueryMiddlewareBuilder
    {
        public Func<IQueryable<TEntity>, IQueryable<TEntity>> Build<TEntity>(
            IResolveFieldContext resolveFieldContext) where TEntity : class
        {
            if (resolveFieldContext == null)
                throw new ArgumentNullException(nameof(resolveFieldContext));

            var limit = resolveFieldContext.GetArgument<int?>(EntityGraphQueryConstants.GraphQueryArgumnet_Limit);

            return queryable =>
            {
                if (limit.HasValue)
                {
                    queryable = queryable.Take(limit.Value);
                }

                return queryable;
            };
        }
    }
}
