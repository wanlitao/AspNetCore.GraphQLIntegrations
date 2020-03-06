using GraphQL.Types;

namespace HEF.GraphQL.EntityQuery
{
    public class OffsetGraphQueryArgumentBuilder : IEntityGraphQueryArgumentBuilder
    {
        protected static readonly QueryArgument OffsetQueryArgument =
            new QueryArgument<IntGraphType> { Name = EntityGraphQueryConstants.GraphQueryArgumnet_Offset_Name };

        public QueryArgument Build<TEntity>() where TEntity : class
        {
            return OffsetQueryArgument;
        }
    }
}
