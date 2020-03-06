using GraphQL.Types;

namespace HEF.GraphQL.EntityQuery
{
    public interface IEntityGraphQueryArgumentBuilder
    {
        QueryArgument Build<TEntity>() where TEntity : class;
    }
}
