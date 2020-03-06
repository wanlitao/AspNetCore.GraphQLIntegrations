using GraphQL.Types;

namespace HEF.GraphQL.EntityQuery
{
    public interface IEntityGraphQueryArgumentsBuilder
    {
        QueryArguments Build<TEntity>() where TEntity : class;
    }
}
