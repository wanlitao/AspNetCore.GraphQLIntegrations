using GraphQL.Types;

namespace HEF.GraphQL.ResourceQuery
{
    public interface IEntityGraphQueryArgumentsBuilder
    {
        QueryArguments Build<TEntity>() where TEntity : class;
    }
}
