using GraphQL.Types;
using System;
using System.Collections.Generic;

namespace HEF.GraphQL.EntityQuery
{
    public class EntityGraphQueryArgumentsBuilder : IEntityGraphQueryArgumentsBuilder
    {
        public EntityGraphQueryArgumentsBuilder(
            IEnumerable<IEntityGraphQueryArgumentBuilder> entityGraphQueryArgumentBuilders)
        {
            EntityGraphQueryArgumentBuilders = entityGraphQueryArgumentBuilders
                ?? throw new ArgumentNullException(nameof(entityGraphQueryArgumentBuilders));
        }

        protected IEnumerable<IEntityGraphQueryArgumentBuilder> EntityGraphQueryArgumentBuilders { get; }

        public QueryArguments Build<TEntity>() where TEntity : class
        {
            var queryArguments = new QueryArguments();
            foreach (var queryArgumnetBuilder in EntityGraphQueryArgumentBuilders)
            {
                queryArguments.Add(queryArgumnetBuilder.Build<TEntity>());
            }

            return queryArguments;
        }
    }
}
