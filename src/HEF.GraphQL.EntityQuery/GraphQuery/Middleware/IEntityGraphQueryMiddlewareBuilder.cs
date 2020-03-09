using GraphQL;
using System;
using System.Linq;

namespace HEF.GraphQL.EntityQuery
{
    public interface IEntityGraphQueryMiddlewareBuilder
    {
        Func<IQueryable<TEntity>, IQueryable<TEntity>> Build<TEntity>(
            IResolveFieldContext resolveFieldContext) where TEntity : class;
    }
}
