using GraphQL.Types;

namespace HEF.GraphQL.ResourceQuery
{
    public interface IEntityGraphTypeBuilder
    {
        ObjectGraphType<TEntity> Build<TEntity>() where TEntity : class;
    }
}
