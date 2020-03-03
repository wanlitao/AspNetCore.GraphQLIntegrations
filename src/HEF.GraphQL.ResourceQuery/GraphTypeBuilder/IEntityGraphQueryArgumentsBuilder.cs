using GraphQL.Types;

namespace HEF.GraphQL.ResourceQuery
{
    public interface IEntityGraphQueryArgumentsBuilder
    {
        QueryArgument BuildOrderBy<TEntity>() where TEntity : class;

        QueryArgument BuildPredicate<TEntity>() where TEntity : class;

        QueryArguments Build<TEntity>() where TEntity : class;
    }
}
