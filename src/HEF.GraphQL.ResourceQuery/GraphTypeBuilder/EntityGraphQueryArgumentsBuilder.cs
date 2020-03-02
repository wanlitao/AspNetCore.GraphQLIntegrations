using GraphQL.Types;
using System;

namespace HEF.GraphQL.ResourceQuery
{
    public class EntityGraphQueryArgumentsBuilder : IEntityGraphQueryArgumentsBuilder
    {
        public QueryArguments Build<TEntity>() where TEntity : class
        {
            throw new NotImplementedException();
        }
    }
}
