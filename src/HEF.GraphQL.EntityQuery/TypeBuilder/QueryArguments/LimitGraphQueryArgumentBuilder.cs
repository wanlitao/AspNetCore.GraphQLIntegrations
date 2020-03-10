using GraphQL.Types;

namespace HEF.GraphQL.EntityQuery
{
    public class LimitGraphQueryArgumentBuilder : IEntityGraphQueryArgumentBuilder
    {
        protected static readonly QueryArgument LimitQueryArgument =
            new QueryArgument<IntGraphType> { Name = EntityGraphQueryConstants.GraphQueryArgumnet_Limit };

        public QueryArgument Build<TEntity>() where TEntity : class
        {
            return LimitQueryArgument;
        }
    }
}
