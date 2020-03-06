using System;
using System.Linq;

namespace HEF.GraphQL.EntityQuery
{
    public interface IEntityGraphQueryBuilder<TEntity> where TEntity : class
    {
        IQueryable<TEntity> Build();

        IEntityGraphQueryBuilder<TEntity> New();

        IEntityGraphQueryBuilder<TEntity> Use(Func<IQueryable<TEntity>, IQueryable<TEntity>> middleware);
    }
}
