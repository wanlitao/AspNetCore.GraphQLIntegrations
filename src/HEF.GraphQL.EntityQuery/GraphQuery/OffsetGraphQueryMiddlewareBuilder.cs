using GraphQL;
using System;
using System.Linq;

namespace HEF.GraphQL.EntityQuery
{
    public class OffsetGraphQueryMiddlewareBuilder : IEntityGraphQueryMiddlewareBuilder
    {
        public Func<IQueryable<TEntity>, IQueryable<TEntity>> Build<TEntity>(
            IResolveFieldContext resolveFieldContext) where TEntity : class
        {
            if (resolveFieldContext == null)
                throw new ArgumentNullException(nameof(resolveFieldContext));

            var offset = resolveFieldContext.GetArgument<int?>(EntityGraphQueryConstants.GraphQueryArgumnet_Offset_Name);

            return queryable =>
            {
                if (offset.HasValue)
                {
                    queryable = queryable.Skip(offset.Value);
                }

                return queryable;
            };
        }
    }
}
